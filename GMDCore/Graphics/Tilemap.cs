using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace GMDCore.Graphics;

public class Tilemap
{
    private readonly Tile[] _tiles;

    public int Rows { get; }
    public int Columns { get; }
    public int Count { get; }
    public Vector2 Scale { get; set; }
    public Tileset Tileset { get; set; }
    public Tileset Topperset { get; set; }
    public float TileWidth => Tileset.TileWidth * Scale.X;
    public float TileHeight => Tileset.TileHeight * Scale.Y;

    public Tilemap(Tileset tileset, int columns, int rows, Tileset topperset = null)
    {
        Tileset = tileset;
        Topperset = topperset;
        Rows = rows;
        Columns = columns;
        Count = Columns * Rows;
        Scale = Vector2.One;
        _tiles = new Tile[Count];
        Array.Fill(_tiles, Tile.Empty);
    }

    public void SetTile(int index, Tile tile)
    {
        _tiles[index] = tile;
    }

    public void SetTile(int column, int row, Tile tile)
    {
        int index = row * Columns + column;
        SetTile(index, tile);
    }

    public Tile GetTile(int index)
    {
        return _tiles[index];
    }

    public Tile GetTile(int column, int row)
    {
        int index = row * Columns + column;
        return GetTile(index);
    }

    public Tile PointToTile(Vector2 point)
    {
        return GetTileAt(point.X, point.Y) ?? 
            throw new ArgumentOutOfRangeException(nameof(point), "Point is outside the bounds of the tilemap.");
    }

    public Vector2 TileToPoint(int column, int row)
    {
        return new Vector2(column * TileWidth, row * TileHeight);
    }

    public bool IsSolidAt(float x, float y)
    {
        return GetTileAt(x, y)?.IsSolid ?? false;
    }

    public float GetTileLeft(float x) => (int)(x / TileWidth) * TileWidth;
    public float GetTileRight(float x) => ((int)(x / TileWidth) + 1) * TileWidth;
    public float GetTileTop(float y) => (int)(y / TileHeight) * TileHeight;
    public float GetTileBottom(float y) => ((int)(y / TileHeight) + 1) * TileHeight;

    private Tile? GetTileAt(float x, float y)
    {
        int column = (int)(x / TileWidth);
        int row = (int)(y / TileHeight);

        if (column < 0 || column >= Columns || row < 0 || row >= Rows)
        {
            return null;
        }

        return GetTile(column, row);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        for (int i = 0; i < Count; i++)
        {
            Tile tileData = _tiles[i];

            int x = i % Columns;
            int y = i / Columns;

            Vector2 position = new(x * TileWidth, y * TileHeight);

            // Draw base tile
            if (tileData.GraphicId >= 0 && Tileset != null)
            {
                TextureRegion baseGraphic = Tileset.GetTile(tileData.GraphicId);
                baseGraphic.Draw(spriteBatch, position, Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 1.0f);
            }

            // Draw topper if it exists
            if (tileData.HasTopper && Topperset != null)
            {
                TextureRegion topperGraphic = Topperset.GetTile(tileData.TopperId);
                topperGraphic.Draw(spriteBatch, position, Color.White, 0.0f, Vector2.Zero, Scale, SpriteEffects.None, 1.0f);
            }
        }
    }

    public static Tilemap FromFile(ContentManager content, string filename)
    {
        string filePath = Path.Combine(content.RootDirectory, filename);

        using Stream stream = TitleContainer.OpenStream(filePath);
        using XmlReader reader = XmlReader.Create(stream);
        XDocument doc = XDocument.Load(reader);
        XElement root = doc.Root;

        // The <Tileset> element contains the information about the tileset
        // used by the tilemap.
        //
        // Example
        // <Tileset region="0 0 100 100" tileWidth="10" tileHeight="10">contentPath</Tileset>
        //
        // The region attribute represents the x, y, width, and height
        // components of the boundary for the texture region within the
        // texture at the contentPath specified.
        //
        // the tileWidth and tileHeight attributes specify the width and
        // height of each tile in the tileset.
        //
        // the contentPath value is the contentPath to the texture to
        // load that contains the tileset
        XElement tilesetElement = root.Element("Tileset");

        string regionAttribute = tilesetElement.Attribute("region").Value;
        string[] split = regionAttribute.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        int x = int.Parse(split[0]);
        int y = int.Parse(split[1]);
        int width = int.Parse(split[2]);
        int height = int.Parse(split[3]);

        int tileWidth = int.Parse(tilesetElement.Attribute("tileWidth").Value);
        int tileHeight = int.Parse(tilesetElement.Attribute("tileHeight").Value);
        string contentPath = tilesetElement.Value;

        // Load the texture 2d at the content path
        Texture2D texture = content.Load<Texture2D>(contentPath);

        // Create the texture region from the texture
        TextureRegion textureRegion = new(texture, x, y, width, height);

        // Create the tileset using the texture region
        Tileset tileset = new(textureRegion, tileWidth, tileHeight);

        // The <Tiles> element contains lines of strings where each line
        // represents a row in the tilemap.  Each line is a space
        // separated string where each element represents a column in that
        // row.  The value of the column is the id of the tile in the
        // tileset to draw for that location.
        //
        // Example:
        // <Tiles>
        //      00 01 01 02
        //      03 04 04 05
        //      03 04 04 05
        //      06 07 07 08
        // </Tiles>
        XElement tilesElement = root.Element("Tiles");

        // Split the value of the tiles data into rows by splitting on
        // the new line character
        string[] rows = tilesElement.Value.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Split the value of the first row to determine the total number of columns
        int columnCount = rows[0].Split(" ", StringSplitOptions.RemoveEmptyEntries).Length;

        // Create the tilemap
        Tilemap tilemap = new(tileset, columnCount, rows.Length);

        // Process each row
        for (int row = 0; row < rows.Length; row++)
        {
            // Split the row into individual columns
            string[] columns = rows[row].Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries);

            // Process each column of the current row
            for (int column = 0; column < columnCount; column++)
            {
                // Get the tileset index for this location
                int tilesetIndex = int.Parse(columns[column]);

                // Create a basic tile wrapper with this graphic
                Tile tile = new() { GraphicId = tilesetIndex };

                // Add that tile to the tilemap at the row and column location
                tilemap.SetTile(column, row, tile);
            }
        }

        return tilemap;
    }
}
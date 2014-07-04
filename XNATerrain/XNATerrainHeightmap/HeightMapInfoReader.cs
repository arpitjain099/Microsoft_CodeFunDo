using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace XNATerrainHeightmap
{
    /// <summary>
    /// This class will load the HeightMapInfo when the game starts. This class needs 
    /// to match the HeightMapInfoWriter.
    /// </summary>
    public class HeightMapInfoReader : ContentTypeReader<XNATerrain>
    {
        protected override XNATerrain Read(ContentReader input, XNATerrain existingInstance)
        {
            var model = input.ReadObject<Model>();
            float terrainScale = input.ReadSingle();
            int width = input.ReadInt32();
            int height = input.ReadInt32();

            var terrainVertices = new VertexPositionNormalTexture[width * height];
            model.Meshes[0].VertexBuffer.GetData<VertexPositionNormalTexture>(terrainVertices);

            float[,] heights = new float[width, height];
            Vector3[,] normals = new Vector3[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    heights[x, z] = input.ReadSingle();
                }
            }
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    normals[x, z] = input.ReadVector3();
                }
            }

            return new XNATerrain(heights, normals, terrainScale, model, terrainVertices);
        }
    }
}

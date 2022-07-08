using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.mineorbit.dungeonsanddungeonscommon;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class CubeTileGrid : MonoBehaviour
    {
        public MeshFilter _meshFilter;


        public LevelObjectData levelObjectType;
        
        private int gridSize = 8;
        public Mesh mesh;

        private bool[] existingCubes;

        public MeshCollider meshCollider;

        private bool initialized = false;
        // Start is called before the first frame update
        void Start()
        {
            Init();
            _meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;
            //_meshFilter.sharedMesh = mesh;
        }

        public void Init(int size = 2)
        {
            if (!initialized)
            {
                initialized = true;
                
                existingCubes = new bool[gridSize * gridSize * gridSize];
                mesh = new Mesh();
                mesh.name = "Grid";
                Vector3[] vertices = new Vector3[gridSize * gridSize * gridSize];
                for (int i = 0; i < gridSize; i++)
                {
                    for (int j = 0; j < gridSize; j++)
                    {
                        for (int k = 0; k < gridSize; k++)
                        {
                            vertices[ConvertCoordinates(i, j, k)] = new Vector3(size*i, size*j, size*k);
                        }
                    }
                }

                mesh.vertices = vertices;
            }
        }

        int ConvertCoordinates(int i, int j, int k)
        {
            return 64 * i + 8 * j + k;
        }

        int[] GetVerticesOfBlock(int x, int y, int z)
        {
            int[] coords = new int[8];
            for (int i = 0; i <= 1; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    for (int k = 0; k <= 1; k++)
                    {
                        coords[4 * i + 2 * j + k] = ConvertCoordinates(x + i, y + j, z + k);
                    }
                }

            }

            return coords;
        }

        /*
        private void OnDrawGizmos () {
            if (mesh == null) {
                return;
            }
            Gizmos.color = Color.black;
            for (int i = 0; i < mesh.vertices.Length; i++) { 
                //  Gizmos.DrawSphere(mesh.vertices[i], 0.1f);
            }
        }
        */
        public bool Exists(int x, int y, int z) => existingCubes[ConvertCoordinates(x, y, z)];


        public void AddCube(int x, int y, int z)
        {
            if (Exists(x, y, z))
            {
                return;
            }

            existingCubes[ConvertCoordinates(x, y, z)] = true;
            int[] ind = GetVerticesOfBlock(x, y, z);

            GameConsole.Log(string.Join(", ", ind));


            if ((x + 1) < gridSize && Exists(x + 1, y, z))
            {
                int[] ind2 = GetVerticesOfBlock(x + 1, y, z);
                RemoveFace(ind2, Face.Front);
            }
            else
            {
                AddFace(ind, Face.Back);
            }



            if ((y + 1) < gridSize && Exists(x, y + 1, z))
            {
                int[] ind2 = GetVerticesOfBlock(x, y + 1, z);
                RemoveFace(ind2, Face.Bottom);
            }
            else
            {
                AddFace(ind, Face.Top);
            }

            if ((z + 1) < gridSize && Exists(x, y, z + 1))
            {
                int[] ind2 = GetVerticesOfBlock(1, y, z + 1);
                RemoveFace(ind2, Face.Right);
            }
            else
            {
                AddFace(ind, Face.Left);
            }


            if (0 <= (x - 1) && Exists(x - 1, y, z))
            {
                int[] ind2 = GetVerticesOfBlock(x - 1, y, z);
                RemoveFace(ind2, Face.Back);
            }
            else
            {
                AddFace(ind, Face.Front);
            }


            if (0 <= (y - 1) && Exists(x, y - 1, z))
            {
                int[] ind2 = GetVerticesOfBlock(x, y - 1, z);
                RemoveFace(ind2, Face.Top);
            }
            else
            {
                AddFace(ind, Face.Bottom);
            }



            if (0 <= (z - 1) && Exists(x, y, z - 1))
            {
                int[] ind2 = GetVerticesOfBlock(x, y, z - 1);
                RemoveFace(ind2, Face.Left);
            }
            else
            {
                AddFace(ind, Face.Right);
            }



            mesh.RecalculateNormals();

        }

        public void RemoveCube(int x, int y, int z)
        {
            if (!Exists(x, y, z))
            {
                return;
            }

            existingCubes[ConvertCoordinates(x, y, z)] = false;
            int[] ind = GetVerticesOfBlock(x, y, z);


            if ((x + 1) < gridSize && Exists(x + 1, y, z))
            {
                int[] ind2 = GetVerticesOfBlock(x + 1, y, z);
                AddFace(ind2, Face.Front);
            }
            else
            {
                RemoveFace(ind, Face.Back);
            }



            if ((y + 1) < gridSize && Exists(x, y + 1, z))
            {
                int[] ind2 = GetVerticesOfBlock(x, y + 1, z);
                AddFace(ind2, Face.Bottom);
            }
            else
            {
                RemoveFace(ind, Face.Top);
            }

            if ((z + 1) < gridSize && Exists(x, y, z + 1))
            {
                int[] ind2 = GetVerticesOfBlock(x, y, z + 1);
                GameConsole.Log("TEST");
                AddFace(ind2, Face.Right);
            }
            else
            {
                RemoveFace(ind, Face.Left);
            }


            if (0 <= (x - 1) && Exists(x - 1, y, z))
            {
                int[] ind2 = GetVerticesOfBlock(x - 1, y, z);
                AddFace(ind2, Face.Back);
            }
            else
            {
                RemoveFace(ind, Face.Front);
            }


            if (0 <= (y - 1) && Exists(x, y - 1, z))
            {
                int[] ind2 = GetVerticesOfBlock(x, y - 1, z);
                AddFace(ind2, Face.Top);
            }
            else
            {
                RemoveFace(ind, Face.Bottom);
            }



            if (0 <= (z - 1) && Exists(x, y, z - 1))
            {
                int[] ind2 = GetVerticesOfBlock(x, y, z - 1);
                AddFace(ind2, Face.Left);
            }
            else
            {
                RemoveFace(ind, Face.Right);
            }


            mesh.RecalculateNormals();
        }


        public enum Face
        {
            Front,
            Back,
            Left,
            Right,
            Top,
            Bottom
        }

        public void RemoveFace(int[] cube_ind, Face face)
        {
            int[] tris = (int[]) mesh.triangles.Clone();
            int start = tris.Length;
            Array.Resize(ref tris, tris.Length + 6);

            int lb = 2;
            int rb = 0;
            int lh = 3;
            int rh = 1;

            switch (face)
            {
                case Face.Front:
                    break;
                case Face.Left:
                    lb = 1;
                    rb = 5;
                    lh = 3;
                    rh = 7;
                    break;
                case Face.Back:

                    lb = 4;
                    rb = 6;
                    lh = 5;
                    rh = 7;
                    break;
                case Face.Right:
                    lb = 2;
                    rb = 6;
                    lh = 0;
                    rh = 4;
                    break;

                case Face.Top:
                    lb = 2;
                    rb = 3;
                    lh = 6;
                    rh = 7;
                    break;


                case Face.Bottom:
                    lb = 0;
                    rb = 4;
                    lh = 1;
                    rh = 5;
                    break;

            }

            int startIndex = 0;
            bool hit = false;
            for (int i = 0; i < tris.Length; i += 6)
            {
                bool cond = tris[i] == cube_ind[lb] &&
                            tris[i + 2] == cube_ind[lh] &&
                            tris[i + 3] == cube_ind[lh] &&
                            tris[i + 4] == cube_ind[rb] &&
                            tris[i + 1] == cube_ind[rb] &&
                            tris[i + 5] == cube_ind[rh];
                if (cond)
                {
                    hit = true;
                    break;
                }

                startIndex += 6;
            }

            if (hit)
            {
                int[] new_tris = new int[tris.Length - 6];
                for (int x = 0; x < startIndex; x++)
                {
                    new_tris[x] = tris[x];
                }

                for (int x = (startIndex + 6); x < tris.Length; x++)
                {
                    new_tris[x - 6] = tris[x];
                }

                tris = new_tris;
            }

            mesh.triangles = tris;
        }

        public void AddFace(int[] cube_ind, Face face)
        {
            int[] tris = (int[]) mesh.triangles.Clone();
            int start = tris.Length;
            Array.Resize(ref tris, tris.Length + 6);

            int lb = 2;
            int rb = 0;
            int lh = 3;
            int rh = 1;

            switch (face)
            {
                case Face.Front:
                    break;
                case Face.Left:
                    lb = 1;
                    rb = 5;
                    lh = 3;
                    rh = 7;
                    break;
                case Face.Back:

                    lb = 4;
                    rb = 6;
                    lh = 5;
                    rh = 7;
                    break;
                case Face.Right:
                    lb = 2;
                    rb = 6;
                    lh = 0;
                    rh = 4;
                    break;

                case Face.Top:
                    lb = 2;
                    rb = 3;
                    lh = 6;
                    rh = 7;
                    break;


                case Face.Bottom:
                    lb = 0;
                    rb = 4;
                    lh = 1;
                    rh = 5;
                    break;

            }

            tris[start] = cube_ind[lb];
            tris[start + 3] = tris[start + 2] = cube_ind[lh];
            tris[start + 4] = tris[start + 1] = cube_ind[rb];
            tris[start + 5] = cube_ind[rh];
            mesh.triangles = tris;
        }
    }
}
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
        
        public int gridSize = 8;
        public Mesh mesh;

        private bool[] existingCubes;

        public MeshCollider meshCollider;

        private bool initialized = false;

        public int cubeSize = 2;
        // Start is called before the first frame update
        void Start()
        {
            Init();
            //_meshFilter.sharedMesh = mesh;
        }

        public void UpdateData()
        {
            Mesh renderedMesh = Split(mesh);
            
            _meshFilter.mesh = renderedMesh;
            meshCollider.sharedMesh = renderedMesh;
        }


        struct FaceData
        {
            public Vector3 x0; //lb
            public Vector3 x1; //rb
            public Vector3 x2; //lh
            public Vector3 x3; //rh
            
        }
        
        public Mesh Split(Mesh m)
        {
            Mesh renderedMesh = new Mesh();
            renderedMesh.name = m.name;
            List<FaceData> faces = new List<FaceData>();
            for (int i = 0;i<m.triangles.Length;i+=6)
            {
                GameConsole.Log("Adding Face");
                FaceData faceData = new FaceData();
                faceData.x0 = m.vertices[m.triangles[i]]; //lb
                faceData.x1 = m.vertices[m.triangles[i + 1]]; //rb
                faceData.x2 = m.vertices[m.triangles[i + 2]]; //lh
                faceData.x3 = m.vertices[m.triangles[i + 5]]; //rh
                /*
                tris[start] = cube_ind[lb];
                tris[start + 3] = tris[start + 2] = cube_ind[lh];
                tris[start + 4] = tris[start + 1] = cube_ind[rb];
                tris[start + 5] = cube_ind[rh];
                */
                faces.Add(faceData);
            }

            Vector3[] new_verts = new Vector3[4*faces.Count];
            int[] new_tris = new int[6*faces.Count];
            int count = 0;
            foreach (var face in faces)
            {
                int tri_start = count *6;
                int vert_start = count * 4;

                new_verts[vert_start] = face.x0; //lb
                new_verts[vert_start + 1] = face.x1; //rb
                new_verts[vert_start + 2] = face.x2; // lh
                new_verts[vert_start + 3] = face.x3; // rh
                
                
                new_tris[tri_start] = vert_start;
                new_tris[tri_start + 3] = new_tris[tri_start + 2] = vert_start + 2;
                new_tris[tri_start + 4] = new_tris[tri_start + 1] = vert_start + 1;
                new_tris[tri_start + 5] = vert_start + 3;

                count += 1;
            }
            renderedMesh.vertices = new_verts;
            renderedMesh.triangles =  new_tris;
            renderedMesh.RecalculateNormals();
            return renderedMesh;
        }

        public void Init(int cSize = 2)
        {
            if (!initialized)
            {
                cubeSize = cSize;
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
                            vertices[ConvertCoordinates(i, j, k)] = new Vector3(cubeSize*i, cubeSize*j, cubeSize*k);
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


        public void AddCube(int x1, int y1, int z1)
        {
            int x = x1 / 2;
            int y = y1 / 2;
            int z = z1 / 2;
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

            UpdateData();
        }

        public void RemoveCube(int x1, int y1, int z1)
        {
            int x = x1 / 2;
            int y = y1 / 2;
            int z = z1 / 2;
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
            UpdateData();
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
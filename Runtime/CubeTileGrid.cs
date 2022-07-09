using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using com.mineorbit.dungeonsanddungeonscommon;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace com.mineorbit.dungeonsanddungeonscommon
{
    public class CubeTileGrid : MonoBehaviour
    {
        public MeshFilter _meshFilter;


        public LevelObjectData levelObjectType;
        
        public int gridSize = 8;

        private bool[] existingCubes;

        public MeshCollider meshCollider;

        private bool initialized = false;

        public int cubeSize = 2;

        public Vector3[] vertices;
        // Start is called before the first frame update
        void Start()
        {
            Init();
            //_meshFilter.sharedMesh = mesh;
        }

        public void UpdateData()
        {
            Mesh renderedMesh = Split();
            
            _meshFilter.mesh = renderedMesh;
            meshCollider.sharedMesh = renderedMesh;
        }

        // Simultaneous Updates

        public static int allowedEntries = 1;
        public bool changeImplemented = true;
        public void FixedUpdate()
        {
            // Currently add atmost one cube per frame
            if (addCubes.Count > 0)
            {
                var ce = addCubes.Dequeue();
                _AddCube(ce.x,ce.y,ce.z);
            }
            if (removeCubes.Count > 0)
            {
                var ce = addCubes.Dequeue();
                _RemoveCube(ce.x,ce.y,ce.z);
            }
            
            if (!changeImplemented && allowedEntries > 0)
            {
                GameConsole.Log("Entered");
                changeImplemented = true;
                allowedEntries--;
                UpdateData();
                allowedEntries++;
            }
        }

        public struct FaceData
        {
            public int x, y, z;
            public Face face;
            public Vector3 x0; //lb
            public Vector3 x1; //rb
            public Vector3 x2; //lh
            public Vector3 x3; //rh
            
        }
        
        public Mesh Split()
        {
            Mesh renderedMesh = new Mesh();
            renderedMesh.name = "grid";
            /*
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
                
                faces.Add(faceData);
            }
             */

            Vector3[] new_verts = new Vector3[4*faces.Count];
            int[] new_tris = new int[6*faces.Count];
            int count = 0;
            foreach (var face in faces)
            {
                int tri_start = count * 6;
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
                vertices = new Vector3[gridSize * gridSize * gridSize];
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


        struct CubeEntry
        {
            public int x;
            public int y;
            public int z;
        }
        Queue<CubeEntry> addCubes = new Queue<CubeEntry>();
        Queue<CubeEntry> removeCubes = new Queue<CubeEntry>();

        public void AddCube(int x1, int y1, int z1)
        {
            CubeEntry ce = new CubeEntry();
            ce.x = x1;
            ce.y = y1;
            ce.z = z1;
            addCubes.Enqueue(ce);
        }

        public void RemoveCube(int x1, int y1, int z1)
        {
            CubeEntry ce = new CubeEntry();
            ce.x = x1;
            ce.y = y1;
            ce.z = z1;
            removeCubes.Enqueue(ce);
        }
        public void _AddCube(int x1, int y1, int z1)
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
                RemoveFace(Face.Front,x+1,y,z);
            }
            else
            {
                AddFace(ind, Face.Back,x,y,z);
            }



            if ((y + 1) < gridSize && Exists(x, y + 1, z))
            {
                RemoveFace(Face.Bottom,x,y+1,z);
            }
            else
            {
                AddFace(ind, Face.Top,x,y,z);
            }

            if ((z + 1) < gridSize && Exists(x, y, z + 1))
            {
                RemoveFace( Face.Right,x,y,z+1);
            }
            else
            {
                AddFace(ind, Face.Left,x,y,z);
            }


            if (0 <= (x - 1) && Exists(x - 1, y, z))
            {
                RemoveFace(Face.Back,x-1,y,z);
            }
            else
            {
                AddFace(ind, Face.Front,x,y,z);
            }


            if (0 <= (y - 1) && Exists(x, y - 1, z))
            {
                RemoveFace(Face.Top,x,y-1,z);
            }
            else
            {
                AddFace(ind, Face.Bottom,x,y,z);
            }



            if (0 <= (z - 1) && Exists(x, y, z - 1))
            {
                RemoveFace(Face.Left,x,y,z-1);
            }
            else
            {
                AddFace(ind, Face.Right,x,y,z);
            }

            changeImplemented = false;
        }

        public void _RemoveCube(int x1, int y1, int z1)
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
                AddFace(ind2, Face.Front,x+1,y,z);
            }
            else
            {
                RemoveFace( Face.Back,x,y,z);
            }



            if ((y + 1) < gridSize && Exists(x, y + 1, z))
            {
                int[] ind2 = GetVerticesOfBlock(x, y + 1, z);
                AddFace(ind2, Face.Bottom,x,y+1,z);
            }
            else
            {
                RemoveFace(Face.Top,x,y,z);
            }

            if ((z + 1) < gridSize && Exists(x, y, z + 1))
            {
                int[] ind2 = GetVerticesOfBlock(x, y, z + 1);
                AddFace(ind2, Face.Right,x,y,z+1);
            }
            else
            {
                RemoveFace(Face.Left,x,y,z);
            }


            if (0 <= (x - 1) && Exists(x - 1, y, z))
            {
                int[] ind2 = GetVerticesOfBlock(x - 1, y, z);
                AddFace(ind2, Face.Back,x-1,y,z);
            }
            else
            {
                RemoveFace(Face.Front,x,y,z);
            }


            if (0 <= (y - 1) && Exists(x, y - 1, z))
            {
                int[] ind2 = GetVerticesOfBlock(x, y - 1, z);
                AddFace(ind2, Face.Top,x,y-1,z);
            }
            else
            {
                RemoveFace(Face.Bottom,x,y,z);
            }



            if (0 <= (z - 1) && Exists(x, y, z - 1))
            {
                int[] ind2 = GetVerticesOfBlock(x, y, z - 1);
                AddFace(ind2, Face.Left,x,y,z-1);
            }
            else
            {
                RemoveFace(Face.Right,x,y,z);
            }

            changeImplemented = false;
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


        public List<FaceData> faces = new List<FaceData>();
        public void AddFace(int[] cube_ind, Face face,int x,int y,int z)
        {

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

            FaceData faceData = new FaceData();
            faceData.x = x;
            faceData.y = y;
            faceData.z = z;
            faceData.face = face;
       
            faceData.x0 = vertices[cube_ind[lb]];
            faceData.x1 = vertices[cube_ind[rb]];
            faceData.x2 = vertices[cube_ind[lh]];
            faceData.x3 = vertices[cube_ind[rh]];
                 
            //lb
            //rb
            //lh
            //rh
            faces.Add(faceData);
        }


        public void RemoveFace(Face face, int x, int y, int z)
        {
            faces.RemoveAll((f) => (f.face == face && f.x == x && f.y == y && f.z ==z));
        }
        /*
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
        */
        
    }
}
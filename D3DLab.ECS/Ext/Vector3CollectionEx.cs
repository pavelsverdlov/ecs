using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace D3DLab.ECS.Ext {
    public static class Vector3CollectionEx {
        unsafe public static Vector3[] Transform(this Vector3[] positions, ref Matrix4x4 matrix) {
            if (positions == null || positions.Length == 0) {
                return Array.Empty<Vector3>();
            }

            var result = new Vector3[positions.Length];
            fixed (Vector3* _pSrc = positions) {
                fixed (Vector3* _pDst = result) {
                    Vector3* pSrc = _pSrc, pDst = _pDst;
                    var end = pSrc + positions.Length;
                    for (; pSrc < end; ++pSrc, ++pDst) {
                        *pDst = Vector3.Transform(*pSrc, matrix);
                    }
                }
            }
            return result;
        }

        unsafe public static Vector3[] CalculateNormals(this Vector3[] positions, int[] indices) {
            var aNormals = new Vector3[positions.Length];
            var aPos = positions.ToArray();
            var aInd = indices.ToArray();
            fixed (Vector3* pNormal = aNormals) {
                fixed (Vector3* pPos = aPos) {
                    fixed (int* pInd = aInd) {
                        for (var i = 0; i < indices.Length; i += 3) {
                            var index0 = *(pInd + i);
                            var index1 = *(pInd + i + 1);
                            var index2 = *(pInd + i + 2);
                            Vector3 u = Vector3.Subtract(*(pPos + index1), *(pPos + index0));
                            Vector3 v = Vector3.Subtract(*(pPos + index2), *(pPos + index0));
                            Vector3 w = Vector3.Cross(u, v);
                            w.Normalize();

                            if (float.IsNaN(w.X)) {

                            }

                            *(pNormal + index0) = Vector3.Add(*(pNormal + index0), w);
                            *(pNormal + index1) = Vector3.Add(*(pNormal + index1), w);
                            *(pNormal + index2) = Vector3.Add(*(pNormal + index2), w);
                        }
                    }
                }
                for (int i = 0; i < aNormals.Length; i++) {
                    *(pNormal + i) = (*(pNormal + i)).Normalized();
                }
            }

            return aNormals;
        }

        unsafe public static List<Vector3> CalculateNormals(this List<Vector3> positions, List<int> indices) {

            var aNormals = new Vector3[positions.Count];
            var aPos = positions.ToArray();
            var aInd = indices.ToArray();
            fixed (Vector3* pNormal = aNormals) {
                fixed (Vector3* pPos = aPos) {
                    fixed (int* pInd = aInd) {
                        for (var i = 0; i < indices.Count; i += 3) {
                            var index0 = *(pInd + i);
                            var index1 = *(pInd + i + 1);
                            var index2 = *(pInd + i + 2);
                            Vector3 u = Vector3.Subtract(*(pPos + index1), *(pPos + index0));
                            Vector3 v = Vector3.Subtract(*(pPos + index2), *(pPos + index0));
                            Vector3 w = Vector3.Cross(u, v);
                            w.Normalize();

                            *(pNormal + index0) = Vector3.Add(*(pNormal + index0), w);
                            *(pNormal + index1) = Vector3.Add(*(pNormal + index1), w);
                            *(pNormal + index2) = Vector3.Add(*(pNormal + index2), w);
                        }
                    }
                }
                for (int i = 0; i < aNormals.Length; i++) {
                    *(pNormal + i) = (*(pNormal + i)).Normalized();
                }
            }

            return aNormals.ToList();
        }

        public static List<Vector3> ToVector3List(this float[] vertex) {
            var v = new List<Vector3>(vertex.Length / 3);
            for (var i = 0; i < vertex.Length - 3; i += 3) {
                v.Add(new Vector3(vertex[i], vertex[i + 1], vertex[i + 2]));
            }
            return v;
        }
        public static List<Vector2> ToVector2List(this float[] vertex) {
            var v = new List<Vector2>(vertex.Length / 2);
            for (var i = 0; i < vertex.Length -2; i += 2) {
                v.Add(new Vector2(vertex[i], vertex[i + 1]));
            }
            return v;
        }

    }
}

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VctorExtensions;

namespace Mel.Editr.Terrainnn
{

#if UNITY_EDITOR
    [CustomEditor(typeof(TerrainObjectDistributor))]
    public class TerrainObjectDistributorEditor : Editor
    {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();
            TerrainObjectDistributor tod = (TerrainObjectDistributor)target;

            GUILayout.Label(string.Format("trees: {0}", tod.treeCount));

            if(GUILayout.Button("Distribute")) {
                tod.distribute();
            }

            if(GUILayout.Button("Add distributables")) {
                tod.addDistributables();
            }

            if(GUILayout.Button("Delete all")) {
                tod.deleteAll();
            }

            if(GUILayout.Button("Debug tree instances")) {
                tod.dbugCurrentTrees();
            }
        }
    }
#endif

    [Serializable]
    public struct DistributionRange
    {
        [MinMaxRange(0f, 1f)]
        public MinMaxRange minMax01;

        public AnimationCurve withinRange01;

        //[Range(0f, 1f)]
        //public float center;
        //[Range(0f, 1f)]
        //public float minFalloff;
        //[Range(0f, 1f)]
        //public float maxFallOff;


        public float evaluate(float sample) {

            if(sample < minMax01.rangeStart || sample > minMax01.rangeEnd) { return 0f; }

            return withinRange01.Evaluate((minMax01.rangeEnd - sample) / Mathf.Max(0.01f, minMax01.difference));

            //center = Mathf.Clamp(center, minMax01.rangeStart, minMax01.rangeEnd);
            //if(sample > center) {
            //    float fallOff = Mathf.Max(.01f, (minMax01.rangeEnd - center) * ( maxFallOff));
            //    return Mathf.Clamp01((minMax01.rangeEnd - sample) / fallOff);
            //} else {
            //    float fallOff = Mathf.Max(.01f, (center - minMax01.rangeStart) * ( minFalloff));
            //    return Mathf.Clamp01((sample - minMax01.rangeStart) / fallOff);
            //}
        }
    }

    [Serializable]
    public class DistributableTree
    {

        public DistributionRange distributionRange;
        private UnityEngine.Object prototype;
        private Terrain terrain;

        public DistributableTree(UnityEngine.Object proto, Terrain terrain) {
            prototype = proto;
            this.terrain = terrain;
        }

        private int getTreeProtoIndex() {
            GameObject protoGO = null;
            if(prototype is GameObject) {
                protoGO = (GameObject)prototype;
            }

            if(!protoGO) {
                Debug.Log("no proto go");
                return -1;
            }
            for(int i = 0; i < terrain.terrainData.treePrototypes.Length; ++i) {
                if (protoGO == terrain.terrainData.treePrototypes[i].prefab) {
                    return i;
                }
            }
            return -1;
        }

        private bool createTreeInstance(Vector3 normalizedPos, out TreeInstance treeInstance) {
            treeInstance = new TreeInstance();
            treeInstance.prototypeIndex = getTreeProtoIndex();
            if(treeInstance.prototypeIndex < 0) { return false; }
            treeInstance.color = Color.white;
            treeInstance.position = normalizedPos;
            treeInstance.heightScale = terrain.patchBoundsMultiplier.y;
            treeInstance.widthScale = terrain.patchBoundsMultiplier.x;
            return true;
        }

        public void addToTerrain(Vector3 normalizedPos) {
            TreeInstance treeInstance;
            if(createTreeInstance(normalizedPos, out treeInstance)) {
                terrain.AddTreeInstance(treeInstance);
            }
        }
    }


    public class TerrainObjectDistributor : MonoBehaviour
    {
        [SerializeField]
        Transform[] prototypes;

        [SerializeField]
        Terrain terrain;

        [SerializeField]
        bool isTree;

        [SerializeField, Header("Object density. Min 5 (enforced elsewhere)")] float spread;
        [SerializeField, Header("Protect against crashing the editor")] int MaxInstantiations = 400;

        [SerializeField] bool debugDontActuallyInstantiate;
        [SerializeField] float noiseScale = 3f;

        [SerializeField] DistributionRange distribution;

        [SerializeField]
        List<DistributableTree> distributables;

        float FindApproxMaxHeight(Terrain terrain, out Vector3 maxPos) {
            float max = 0f;
            int maxZIncr = 10, minZIncr = 3;
            int zIncr = minZIncr;
            maxPos = Vector3.zero;
            Vector3 pos;
            float y;
            int xIncr = (int)(terrain.terrainData.bounds.size.x / 10f);
            for (int x = 0; x < (int)terrain.terrainData.bounds.size.x; x += xIncr) {
                for (int z = 0; z < (int)terrain.terrainData.bounds.size.z; z += zIncr) {
                    pos = new Vector3(x, 0f, z);
                    pos += terrain.GetPosition();
                    y = terrain.SampleHeight(pos);
                    if(y > max) {
                        max = y;
                        maxPos = pos;
                        maxPos.y += y;
                        zIncr = Mathf.Clamp(zIncr / 2, minZIncr, maxZIncr);
                    } else {
                        zIncr = Mathf.Min(maxZIncr, zIncr + 1);
                    }
                }
            }
            return max;
        }

        public void addDistributables() {
            distributables.Clear();
            foreach(TreePrototype tpro in terrain.terrainData.treePrototypes) {
                var dist = new DistributableTree(tpro.prefab, terrain);
                distributables.Add(dist);
            }

        }

        public void dbugCurrentTrees() {
            foreach(TreeInstance ti in terrain.terrainData.treeInstances) {
                Debug.Log(string.Format("tree inst: {0}", ti.position.ToString()));
            }
        }

        public int treeCount {
            get { return terrain.terrainData.treeInstanceCount; }
        }

        internal void distribute() {
            _distribute();
            terrain.Flush();
            Debug.Log(terrain.terrainData.treeInstances.Length);
        }

        internal void _distribute() {

            Transform folder = terrain.transform.Find("Folder");
            if(!folder) {
                GameObject folderGo = new GameObject("Folder");
                folder = folderGo.transform;
                folder.transform.position = terrain.transform.position;
                folder.SetParent(terrain.transform);
            }

            Bounds bounds = terrain.terrainData.bounds;
            print(bounds.ToString());

            spread = Mathf.Max(1f, spread);

            int gridX = (int)(bounds.size.x / spread);
            int gridZ = (int)(bounds.size.z / spread);

            int instantiations = 0;

            Vector3 rootPos = terrain.GetPosition(); // terrainGO.transform.position;
            Vector3 pos, nudge;

            print(terrain.patchBoundsMultiplier);

            float[,] noise = Simplex.Noise.Calc2D((int)bounds.size.x, (int)bounds.size.z, noiseScale);

            //Vector3 maxHeightPosition = Vector3.zero;
            float maxHeight = bounds.size.y; // FindApproxMaxHeight(terrain, out maxHeightPosition);
            

            for(int x = 0; x < gridX; x++) {
                for(int z = 0; z < gridZ; z++) {

                    //TODO: add 'tree / detail' meshes instead of objects
                    Vector3 relPos = new Vector3(x * spread, 0f, z * spread);
                    pos = relPos;
                    nudge = new Vector3(UnityEngine.Random.Range(-spread / 2f, spread / 2f), 0f, UnityEngine.Random.Range(-spread / 2f, spread / 2f));
                    pos += nudge;
                    float dbugDataHeight = terrain.terrainData.GetHeight((int)pos.x, (int)pos.z);
                    pos += rootPos;
                    pos.y = terrain.SampleHeight(pos);

                    float relHeight = pos.y - rootPos.y;
                    float sample = relHeight / maxHeight;
                    float chances = distribution.evaluate(sample) * 256f;

                    float noiseVal = noise[(int)relPos.x, (int)relPos.z];
                    
                    Transform proto = prototypes[0];
                    if (chances > noiseVal) {
                        if (!debugDontActuallyInstantiate) {
                            if (isTree) {
                                //TreeInstance treeInst = new TreeInstance();
                                //TreeInstance treeInst = proto.GetComponent<TreeInstance>();
                                //if(!treeInst) {
                                //    print("No tree instance"); return;
                                //}
                                //terrain.AddTreeInstance(treeInst);
                            }
                            else {
                                distributables[0].addToTerrain(normalizedTerrainPos(pos));
                                //Transform clone = Instantiate<Transform>(proto, pos, Quaternion.identity);
                                //clone.SetParent(folder);
                            }

                            if (++instantiations >= MaxInstantiations) {
                                print("hit max instantions: " + MaxInstantiations);
                                return;
                            }
                        }
                    }


                }
            }

        }

        Vector3 normalizedTerrainPos(Vector3 global) {
            return (global - terrain.GetPosition()).divide(terrain.terrainData.bounds.size);
        }

        internal void deleteAll() {
            Transform folder = terrain.transform.Find("Folder");
            if(folder) {
                foreach (Transform child in folder.GetComponentsInChildren<Transform>()) {
                    DestroyImmediate(child.gameObject);
                }
            }

            //delete tree instances
            terrain.terrainData.treeInstances = new TreeInstance[0];
        }
    }

}

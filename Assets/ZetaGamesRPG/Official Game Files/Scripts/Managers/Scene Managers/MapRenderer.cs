using System.Collections;
using UnityEngine;

namespace ZetaGames.RPG {
    public class MapRenderer : MonoBehaviour {
        /*
        //private MapManager mapManager;
        //private ZetaGrid<List<WorldTile>> chunkGrid;
        private int curChunkX = 0;
        private int curChunkY = 0;
        private int lastChunkX = 0;
        private int lastChunkY = 0;
        private float timer = 1;
        private bool initialized;
        private Vector3Int tilePos;

        // Start is called before the first frame update
        void Start() {
            MapManager.Instance.GetChunkGrid().GetXY(transform.position, out curChunkX, out curChunkY);

            lastChunkX = curChunkX;
            lastChunkY = curChunkY;
        }

        // Update is called once per frame
        void Update() {
            if (MapManager.Instance.initialized) {
                if (!initialized) {
                    StartCoroutine(InitializeStartingChunks());
                }

                //TODO: Create a callback event on chunk position change
                if (timer >= 0.5) {
                    MapManager.Instance.GetChunkGrid().GetXY(transform.position, out curChunkX, out curChunkY);
                    //Debug.Log("Current: (" + curChunkX + ", " + curChunkY + ") || Last: (" + lastChunkX + ", " + lastChunkY + ")");
                    if (curChunkX != lastChunkX || curChunkY != lastChunkY) {
                        // if moving left
                        if (curChunkX < lastChunkX) {

                            // unload opposite side chunks
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(lastChunkX + 1, curChunkY)) {
                                UnloadChunk(lastChunkX + 1, curChunkY);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(lastChunkX + 1, curChunkY + 1)) {
                                UnloadChunk(lastChunkX + 1, curChunkY + 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(lastChunkX + 1, curChunkY - 1)) {
                                UnloadChunk(lastChunkX + 1, curChunkY - 1);
                            }

                            //load leftside chunks
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY)) {
                                LoadChunk(curChunkX - 1, curChunkY);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY + 1)) {
                                LoadChunk(curChunkX - 1, curChunkY + 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY - 1)) {
                                LoadChunk(curChunkX - 1, curChunkY - 1);
                            }

                            lastChunkX = curChunkX;
                        } else if (curChunkX > lastChunkX) {

                            // unload opposite side chunks
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(lastChunkX - 1, curChunkY)) {
                                UnloadChunk(lastChunkX - 1, curChunkY);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(lastChunkX - 1, curChunkY + 1)) {
                                UnloadChunk(lastChunkX - 1, curChunkY + 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(lastChunkX - 1, curChunkY - 1)) {
                                UnloadChunk(lastChunkX - 1, curChunkY - 1);
                            }

                            //load rightside chunks
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY)) {
                                LoadChunk(curChunkX + 1, curChunkY);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY + 1)) {
                                LoadChunk(curChunkX + 1, curChunkY + 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY - 1)) {
                                LoadChunk(curChunkX + 1, curChunkY - 1);
                            }

                            lastChunkX = curChunkX;
                        }
                        
                    //}

                    //if (curChunkY != lastChunkY) {
                        // if moving down
                        if (curChunkY < lastChunkY) {

                            // unload opposite side chunks
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX, lastChunkY + 1)) {
                                UnloadChunk(curChunkX, lastChunkY + 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, lastChunkY + 1)) {
                                UnloadChunk(curChunkX + 1, lastChunkY + 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, lastChunkY + 1)) {
                                UnloadChunk(curChunkX - 1, lastChunkY + 1);
                            }

                            //load lower chunks
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX, curChunkY - 1)) {
                                LoadChunk(curChunkX, curChunkY - 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY - 1)) {
                                LoadChunk(curChunkX + 1, curChunkY - 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY - 1)) {
                                LoadChunk(curChunkX - 1, curChunkY - 1);
                            }

                            lastChunkY = curChunkY;
                        } else if (curChunkY > lastChunkY) {

                            // unload opposite side chunks
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX, lastChunkY - 1)) {
                                UnloadChunk(curChunkX, lastChunkY - 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, lastChunkY - 1)) {
                                UnloadChunk(curChunkX + 1, lastChunkY - 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, lastChunkY - 1)) {
                                UnloadChunk(curChunkX - 1, lastChunkY - 1);
                            }

                            //load upper chunks
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX, curChunkY + 1)) {
                                LoadChunk(curChunkX, curChunkY + 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY + 1)) {
                                LoadChunk(curChunkX + 1, curChunkY + 1);
                            }
                            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY + 1)) {
                                LoadChunk(curChunkX - 1, curChunkY + 1);
                            }

                            lastChunkY = curChunkY;
                        }
                    }
                    timer = 0;
                } else {
                    timer += Time.deltaTime;
                }
            }
        }

        private void LoadChunk(int x, int y) {
            foreach (WorldTile worldTile in MapManager.Instance.GetChunkGrid().GetGridObject(x, y)) {
                // if tile is already loaded, then skip
                if (worldTile.loaded) {
                    continue;
                }

                worldTile.loaded = true;

                if (worldTile.HasTileObjectPool()) {
                    StartCoroutine(LoadPooledObject(worldTile));
                }

                //StartCoroutine(mapManager.LoadSpriteAsync(worldTile, mapManager.mapList[worldTile.tilemap]));
            }
        }

        private void UnloadChunk(int x, int y) {
            //Debug.Log("Unloading: (" + x + ", " + y + ")");
            foreach (WorldTile worldTile in MapManager.Instance.GetChunkGrid().GetGridObject(x, y)) {
                // if tile is already unloaded, then skip
                if (!worldTile.loaded) {
                    continue;
                }

                tilePos.x = worldTile.x;
                tilePos.y = worldTile.y;
                worldTile.loaded = false;

                if (worldTile.HasTileObjectPool()) {
                    StartCoroutine(UnloadPooledObject(worldTile));
                }

                //mapManager.mapList[worldTile.tilemap].SetTile(tilePos, null);
            }
        }

        private IEnumerator InitializeStartingChunks() {
            yield return new WaitForSeconds(2);

            //load current chunk
            LoadChunk(curChunkX, curChunkY);

            //load leftside chunks
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY)) {
                LoadChunk(curChunkX - 1, curChunkY);
            }
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY + 1)) {
                LoadChunk(curChunkX - 1, curChunkY + 1);
            }
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY - 1)) {
                LoadChunk(curChunkX - 1, curChunkY - 1);
            }

            //load rightside chunks
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY)) {
                LoadChunk(curChunkX + 1, curChunkY);
            }
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY + 1)) {
                LoadChunk(curChunkX + 1, curChunkY + 1);
            }
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY - 1)) {
                LoadChunk(curChunkX + 1, curChunkY - 1);
            }

            //load lower chunks
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX, curChunkY - 1)) {
                LoadChunk(curChunkX, curChunkY - 1);
            }
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY - 1)) {
                LoadChunk(curChunkX + 1, curChunkY - 1);
            }
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY - 1)) {
                LoadChunk(curChunkX - 1, curChunkY - 1);
            }

            //load upper chunks
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX, curChunkY + 1)) {
                LoadChunk(curChunkX, curChunkY + 1);
            }
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX + 1, curChunkY + 1)) {
                LoadChunk(curChunkX + 1, curChunkY + 1);
            }
            if (MapManager.Instance.GetChunkGrid().IsWithinGridBounds(curChunkX - 1, curChunkY + 1)) {
                LoadChunk(curChunkX - 1, curChunkY + 1);
            }

            initialized = true;
        }

        private IEnumerator LoadPooledObject(WorldTile worldTile) {
            yield return new WaitForSeconds(Random.Range(0f, 0.5f));
            worldTile.InstantiatePooledObject();
        }

        private IEnumerator UnloadPooledObject(WorldTile worldTile) {
            yield return new WaitForSeconds(Random.Range(0f, 0.5f));
            worldTile.RemovePooledObject();
        }
        */
    }
}

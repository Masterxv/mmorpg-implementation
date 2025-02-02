﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameCommon.SerializedObjects;
using GameSceneScripts;
using PubSub;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

namespace MGFClient
{
    [Serializable]
    public class GameData : MonoBehaviour
    {
        public static GameData Instance = null;

        [SerializeField] public Character selectedCharacter;

        [SerializeField] public List<Character> characters;

        [SerializeField] public Region region;

        [SerializeField] public PlayerChannel channel;

        // data structure for subscription on message
        public List<NpcCharacter> npcCharacters;
        public List<Character> playersCharacters;

        //data for render entities and players
        public List<Entity> entities;
        public List<Player> players;
        public List<EntityChannel> pubSubActorsEntities;
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                //Destroy object duplicate
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            players = new List<Player>();
            entities = new List<Entity>();
            
            npcCharacters = new List<NpcCharacter>();
            playersCharacters = new List<Character>();
            
            pubSubActorsEntities = new List<EntityChannel>();
        }


        void Update()
        {
            lock (entities)
            {
                entities.RemoveAll(x => x.mustBeDestroyed);
            }
            
            lock (npcCharacters)
            {
                foreach (var npcCharacter in npcCharacters)
                {
                    SpawnNpc(npcCharacter);
                }
            }

            lock (playersCharacters)
            {
                foreach (var playerCharacter in playersCharacters)
                {
                    SpawnCharacter(playerCharacter);
                }
            }
        }

        private void SpawnNpc(NpcCharacter npcCharacter)
        {
            if (entities.FirstOrDefault(x => x.StartPosition.X == npcCharacter.NpcTemplate.StartPosition.X &&
                                             x.StartPosition.Y == npcCharacter.NpcTemplate.StartPosition.Y &&
                                             x.StartPosition.Z == npcCharacter.NpcTemplate.StartPosition.Z) != null)
                return;

            Vector3 npcPosition = new Vector3(npcCharacter.NpcTemplate.Position.X, npcCharacter.NpcTemplate.Position.Y,
                npcCharacter.NpcTemplate.Position.Z);
            Vector3 npcRotation = new Vector3(npcCharacter.NpcTemplate.Rotation.X, npcCharacter.NpcTemplate.Rotation.Y,
                npcCharacter.NpcTemplate.Rotation.Z);
            npcPosition.y = Terrain.activeTerrain.SampleHeight(npcPosition);
            Debug.Log(npcPosition);
            var characterPrefab = Resources.Load(npcCharacter.NpcTemplate.Prefab) as GameObject;


            var obj = Instantiate(characterPrefab, npcPosition, Quaternion.Euler(npcRotation));
            obj.name = npcCharacter.NpcTemplate.Identifier.ToString();    
            if (obj != null)
            {
                var entity = obj.AddComponent<Entity>();

                entity.Position = npcPosition;
                entity.StartPosition = npcCharacter.NpcTemplate.StartPosition;
                entity.EntityName = npcCharacter.NpcTemplate.Name;
                entity.Identifier = npcCharacter.NpcTemplate.Identifier;    
                
                pubSubActorsEntities.Add(new EntityChannel(npcCharacter.NpcTemplate.Identifier.ToString()));
                entities.Add(entity);
            }
        }

        private void SpawnCharacter(Character character)
        {
            if (players.FirstOrDefault(x => x.CharacterName.Equals(character.Name)) != null ||
                selectedCharacter.Name.Equals(character.Name)) return;

            var characterName = character.Name;
            var posX = character.Loc_X;
            var posZ = character.Loc_Z;
            var posY = Terrain.activeTerrain.terrainData.GetHeight((int) posX, (int) posZ);

            Vector3 characterPosition = new Vector3(posX, posY, posZ);
            characterPosition.y = Terrain.activeTerrain.SampleHeight(characterPosition);

            //to do set by class => prefab
            var characterPrefab = Resources.Load("Hammer Warrior") as GameObject;
            var obj = Instantiate(characterPrefab, characterPosition, Quaternion.identity);
            if (obj != null)
            {
                var player = obj.AddComponent<Player>();
                player.CharacterName = characterName;
                pubSubActorsEntities.Add(new EntityChannel(character.Name));
                Instance.players.Add(player);
            }
        }
        
        void OnGUI()
        {
            GUI.Label(new Rect(0, 0, 100, 100), (Convert.ToString((int)(1.0f / Time.smoothDeltaTime)+" FPS")));
            GUI.Label(new Rect(0, 15, 100, 100), PhotonEngine.Instance.Peer.RoundTripTime.ToString()+" RTT");
        }
    }
}
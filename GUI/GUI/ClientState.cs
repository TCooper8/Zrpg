﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zrpg.Game;

namespace GUI
{
    public class ClientState
    {
        // Singleton pattern for the application.
        public static ClientState state = new ClientState();

        private IGameClient gameClient;

        private string username;
        private string clientId;
        private Garrison garrison;
        private List<Hero> heroes = new List<Hero>();
        private Dictionary<string, Item> items;
        private Dictionary<string, ItemRecord> itemRecords;

        private ClientState()
        {
            this.gameClient = GameClient.RESTClient("http://localhost:8080");
            this.items = new Dictionary<string, Item>();
            this.itemRecords = new Dictionary<string, ItemRecord>();
        }

        private static T EnsureDefined<T>(T val, string msg)
        {
            if (val == null)
            {
                throw new Exception(msg);
            }
            return val;
        }

        public void Authenticate(string username, string password)
        {
            this.username = username;
            // TODO: Lookup the clientId from the server.
            this.clientId = username;
        }

        public async Task<AddGarrisonReply> CreateGarrison(Race race, Faction faction)
        {
            var name = String.Format("{0}'s Garrison", Username);
            var reply = await gameClient.AddGarrison(
                ClientId,
                name,
                race,
                faction
            );
            return reply;
        }

        public async Task<AddHeroReply> AddHero(string clientID, string name, Race race, Faction faction, Gender gender, HeroClass heroClass)
        {
            var reply = await gameClient.AddHero(new AddHero(
                clientID,
                name,
                race,
                faction,
                gender,
                heroClass              
            ));
            return reply;
        }

        public async Task<GetClientGarrisonReply> GetGarrison()
        {
            var reply = await gameClient.GetClientGarrison(this.ClientId);
            if (reply.IsSuccess)
            {
                // Set the client state's garrison.
                var garrison = ((GetClientGarrisonReply.Success)reply).Item;
                this.garrison = garrison;
            }

            return reply;
        }

        public async Task<GetHeroArrayReply> GetHeroes()
        {
            await GetGarrison();
            var reply = await gameClient.GetHeroArray(garrison.stats.heroes);

            return reply;
        }

        public async Task<List<Region>> GetOwnedRegions()
        {
            var regions = new List<Region>();
            foreach (var id in garrison.ownedRegions)
            {
                var reply = await gameClient.GetRegion(id);
                if (reply.IsSuccess)
                {
                    var success = (GetRegionReply.Success)reply;
                    regions.Add(success.Item);
                }
                else
                {
                    var msg = String.Format(
                        "Expected GetRegionReply.Success but got {0}",
                        reply
                    );
                    throw new Exception(msg);
                }
            }

            return regions;
        }

        public async Task<List<Zone>> GetRegionZones(Region region)
        {
            var zones = new List<Zone>();
            foreach (var zoneId in region.zones)
            {
                var reply = await gameClient.GetZone(zoneId);
                if (reply.IsSuccess)
                {
                    var success = (GetZoneReply.Success)reply;
                    zones.Add(success.Item);
                }
                else
                {
                    var msg = String.Format(
                        "Expected GetRegionReply.Success but got {0}",
                        reply
                    );
                    throw new Exception(msg);
                }
            }

            return zones;
        }

        public List<string> GetOwnedZoneIds()
        {
            return Garrison.ownedZones.ToList<string>();
        }

        public async Task<List<Zone>> GetOwnedZones()
        {
            var zones = new List<Zone>();
            foreach (var zoneId in garrison.ownedZones)
            {
                var reply = await gameClient.GetZone(zoneId);
                if (reply.IsSuccess)
                {
                    var success = (GetZoneReply.Success)reply;
                    zones.Add(success.Item);
                }
                else
                {
                    var msg = String.Format(
                        "Expected GetRegionReply.Success but got {0}",
                        reply
                    );
                    throw new Exception(msg);
                }
            }
            return zones;
        }

        public async Task<HeroInventory> GetHeroInventory(string heroId)
        {
            var reply = await gameClient.GetHeroInventory(heroId);
            if (reply.IsSuccess)
            {
                var success = (GetHeroInventoryReply.Success)reply;
                // Dig through the inventory and get all of the items.
                var inventory = success.Item;
                foreach (var pane in inventory.panes)
                {
                    foreach (var slot in pane.slots)
                    {
                        var option = slot.itemRecordId;
                        if (option.IsGameSome)
                        {
                            var recordId = ((GameOption<string>.GameSome)option).Item;
                            var data = await gameClient.GetItem(recordId);
                            var item = data.Item2;
                            var record = data.Item1;

                            if (! items.ContainsKey(item.id))
                            {
                                items.Remove(item.id);
                            }
                            if (itemRecords.ContainsKey(record.id))
                            {
                                itemRecords.Remove(record.id);
                            }
                            items.Add(item.id, item);
                            itemRecords.Add(record.id, record);
                        }
                    }
                }

                return success.Item;
            }
            else
            {
                var msg = String.Format(
                    "Expected GetHeroInventory.Success but got {0}",
                    reply
                );
                throw new Exception(msg);
            }
        }

        public async Task<Tuple<ItemRecord, Item>> GetItemRecordData(string recordId)
        {
            ItemRecord record;
            if (itemRecords.TryGetValue(recordId, out record))
            {
                Item item;
                if (items.TryGetValue(recordId, out item))
                {
                    return new Tuple<ItemRecord, Item>(record, item);
                }
            }

            var data = await gameClient.GetItem(recordId);
            try
            {
                items.Add(data.Item2.id, data.Item2);
            }
            catch (Exception e) { }
            try
            {
                itemRecords.Add(data.Item1.id, data.Item1);
            }
            catch (Exception e) { }

            return data;
        }

        public async Task<List<Quest>> GetZoneQuests(string zoneId)
        {
            var quests = await gameClient.GetZoneQuests(zoneId);
            return quests.ToList();
        }

        public async Task<List<AssetPositionInfo>> GetZoneAssetPositionInfo(IEnumerable<string> zoneIds)
        {
            var infos = new List<AssetPositionInfo>();
            foreach (var id in zoneIds)
            {
                var reply = await gameClient.GetZoneAssetPositionInfo(id);
                infos.Add(reply);
            }
            return infos;
        }

        public string ClientId { get { return EnsureDefined<string>(clientId, "clientId is not defined"); } }
        public string Username { get { return EnsureDefined<string>(username, "username is not defined"); } }
        public Garrison Garrison { get { return EnsureDefined<Garrison>(garrison, "garrison is not defined"); } }
        public List<Hero> Heroes { get { return EnsureDefined<List<Hero>>(heroes, "hero is not defined"); } }
    }
}

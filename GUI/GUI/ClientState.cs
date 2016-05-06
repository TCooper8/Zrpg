using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
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
        private int gameTime = 0;
        private Dictionary<string, Hero> heroes;
        private Dictionary<string, Item> items;
        private Dictionary<string, ItemRecord> itemRecords;
        private Dictionary<string, Quest> quests;
        private Dictionary<string, QuestRecord> heroQuestRecords;
        private Dictionary<string, QuestRecord> questRecords;

        private ClientState()
        {
            this.gameClient = GameClient.RESTClient("http://localhost:8080");
            this.items = new Dictionary<string, Item>();
            this.itemRecords = new Dictionary<string, ItemRecord>();
            this.quests = new Dictionary<string, Quest>();
            this.heroes = new Dictionary<string, Hero>();
            this.heroQuestRecords = new Dictionary<string, QuestRecord>();
            this.questRecords = new Dictionary<string, QuestRecord>();
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

        public async Task<int> GetGameTime(bool cache)
        {
            if (cache)
            {
                return this.gameTime;
            }

            var gameTime = await gameClient.GetGameTime();
            this.gameTime = gameTime;
            return gameTime;
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

        public async Task<Hero> GetHero(string heroId, bool cache)
        {
            if (cache)
            {
                Hero hero;
                if (heroes.TryGetValue(heroId, out hero))
                {
                    return hero;
                }
            }

            var reply = await gameClient.GetHero(heroId);
            if (reply.IsSuccess)
            {
                Hero hero = (reply as GetHeroReply.Success).Item;
                this.heroes.Remove(heroId);
                this.heroes.Add(heroId, hero);
                return hero;
            }
            else if (reply.IsEmpty)
            {
                throw new Exception("No such hero");
            }
            else
            {
                throw new Exception("Unhandled reply case for GetHeroReply");
            }
        }

        public async Task<GetHeroArrayReply> GetHeroes()
        {
            await GetGarrison();
            var reply = await gameClient.GetHeroArray(garrison.stats.heroes);

            return reply;
        }

        public async Task<Tuple<QuestRecord, Quest>> GetHeroQuest(string heroId, bool cache)
        {
            Quest quest;
            QuestRecord record;

            if (cache)
            {
                if (heroQuestRecords.TryGetValue(heroId, out record))
                {
                    if (quests.TryGetValue(record.questId, out quest))
                    {
                        return new Tuple<QuestRecord, Quest>(record, quest);
                    }
                }
            }

            var reply = await gameClient.GetHeroQuest(heroId);
            if (reply.IsSuccess)
            {
                var data = (reply as GetHeroQuestReply.Success);
                quest = data.Item2;
                record = data.Item1;

                heroQuestRecords.Remove(heroId);
                heroQuestRecords.Add(heroId, record);

                questRecords.Remove(record.id);
                questRecords.Add(record.id, record);

                quests.Remove(quest.id);
                quests.Add(quest.id, quest);

                return new Tuple<QuestRecord, Quest>(record, quest);
            }
            else if (reply.IsEmpty)
            {
                throw new Exception("Hero does not have a quest!");
            }
            else
            {
                throw new Exception("Unhandled reply case for GetHeroQuestReply");
            }
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
                            Debug.WriteLine("Getting item from record {0}", recordId, null);
                            var data = await gameClient.GetItem(recordId);
                            Debug.WriteLine("Got item from record");

                            var item = data.Item2;
                            var record = data.Item1;

                            items.Remove(item.id);
                            itemRecords.Remove(record.id);
                            items.Add(item.id, item);
                            itemRecords.Add(record.id, record);
                            Debug.WriteLine("Cached item {0}", item.id, null);
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

            foreach (var quest in quests)
            {
                this.quests.Remove(quest.id);
                this.quests.Add(quest.id, quest);
            }

            return quests.ToList();
        }

        public async Task BeginHeroQuest(string heroId, string questId)
        {
            var reply = await gameClient.HeroBeginQuest(heroId, questId);
            if (reply.IsSuccess)
            {
                return;
                throw new Exception(String.Format("Expected Success but got {0}", reply));
            }
            else if (reply.IsHeroDoesNotExist)
            {
                throw new Exception("Hero does not exist");
            }
            else if (reply.IsHeroIsQuesting)
            {
                // Notify player that hero is already questing.
                var dialogue = new Windows.UI.Popups.MessageDialog("Hero is already on a quest!");
                await dialogue.ShowAsync();
            }
            else if (reply.IsQuestDoesNotExist)
            {
                throw new Exception("Quest does not exist");
            }
            else
            {
                throw new Exception("Unhandled reply from HeroBeginQuest");
            }
        }

        public Quest GetQuest(string questId)
        {
            Quest quest;
            if (this.quests.TryGetValue(questId, out quest))
            {
                return quest;
            }

            throw new Exception("Cannot get quest");
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
    }
}

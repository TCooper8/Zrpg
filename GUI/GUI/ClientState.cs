using System;
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

        private ClientState()
        {
            this.gameClient = GameClient.RESTClient("http://localhost:8080");
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

        public string ClientId { get { return EnsureDefined<string>(clientId, "clientId is not defined"); } }
        public string Username { get { return EnsureDefined<string>(username, "username is not defined"); } }
        public Garrison Garrison { get { return EnsureDefined<Garrison>(garrison, "garrison is not defined"); } }
    }
}

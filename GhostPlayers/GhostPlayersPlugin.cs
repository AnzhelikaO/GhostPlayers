#region Using
using System;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
#endregion
namespace GhostPlayers
{
    [ApiVersion(2, 1)]
    public class GhostPlayersPlugin : TerrariaPlugin
    {
        #region Description

        public override string Name => "GhostPlayers";
        public override string Author => "Anzhelika";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public override string Description => "Hides player from other players.";

        #endregion
        #region Constructor

        public GhostPlayersPlugin(Main game)
            : base(game)
        {
            for (int i = 0; i < Main.maxPlayers; i++)
                for (int j = 0; j < Main.maxPlayers; j++)
                    GPAPI.CanSee[i, j] = true;
        }

        #endregion

        #region Initialize, Dispose

        public override void Initialize()
        {
            ServerApi.Hooks.NetSendBytes.Register(this, OnSendBytes, 10000);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
        }
        
        protected override void Dispose(bool Disposing)
        {
            if (Disposing)
                ServerApi.Hooks.NetSendBytes.Deregister(this, OnSendBytes);
            base.Dispose(Disposing);
        }

        #endregion

        #region OnSendBytes

        private void OnSendBytes(SendBytesEventArgs args)
        {
            byte seer = (byte)args.Socket.Id, seen;
            int bOffset = args.Offset + 3;
            switch (args.Buffer[args.Offset + 2])
            {
                case 4: case 5: case 12: case 13: case 14: case 16: case 30:
                case 35: case 36: case 40: case 41: case 42: case 43: case 45:
                case 50: case 51: case 55: case 58: case 62: case 66: case 80:
                case 84: case 95: case 96: case 99: case 102: case 115: case 117:
                case 118: case 120: case 121: case 124: case 125: case 128:
                case 134: case 135: case 139: case 147:
                    seen = args.Buffer[bOffset];
                    break;
                case 22: case 24: case 29: case 70:
                    seen = args.Buffer[bOffset + 2];
                    break;
                // UpdateNPC with Target (PlayerID), but we still want mobs to be updated
                //case 23:
                    //seen = args.Buffer[bOffset + 18];
                    //break;
                case 27:
                    seen = args.Buffer[bOffset + 18];
                    break;
                case 47:
                    seen = args.Buffer[args.Offset + args.Count - 2];
                    break;
                case 61: // 2-byte PlayerID
                    seen = args.Buffer[bOffset];
                    break;
                case 65:
                    byte flag = args.Buffer[bOffset];
                    if (((flag & 1) == 0) && ((flag & 4) == 0))
                        return;
                    seen = args.Buffer[bOffset + 1];
                    break;
                // Fuck modules, idc
                //case 82:
                    //break;
                case 91:
                    if (args.Buffer[bOffset + 4] == 255)
                        return;
                    seen = args.Buffer[bOffset + 5];
                    break;
                case 108:
                    seen = args.Buffer[bOffset + 14];
                    break;
                case 110: case 122:
                    seen = args.Buffer[bOffset + 4];
                    break;
                default:
                    return;
            }

            if (!GPAPI.CheckCanSee(seer, seen))
                args.Handled = true;
        }

        #endregion
        #region OnServerLeave

        private void OnServerLeave(LeaveEventArgs args)
        {
            int who = args.Who;
            for (int i = 0; i < Main.maxPlayers; i++)
                GPAPI.CanSee[who, i] = GPAPI.CanSee[i, who] = true;
        }

        #endregion
    }
}
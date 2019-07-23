#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;
#endregion
namespace GhostPlayers
{
    public static class GPAPI
    {
        internal static bool[,] CanSee = new bool[Main.maxPlayers, Main.maxPlayers];
        #region All

        public static readonly byte[] All = Enumerable.Range(0, Main.maxPlayers)
                                                      .Select(i => (byte)i)
                                                      .ToArray();

        public static byte[] AllExcept(byte Player)
        {
            byte[] all = new byte[Main.maxPlayers - 1];

            int a = 0;
            for (byte i = 0; i < Main.maxPlayers; i++)
                if (i != Player)
                    all[a++] = i;

            return all;
        }
        
        public static byte[] AllExcept(byte Player1, byte Player2)
        {
            byte[] all = new byte[Main.maxPlayers - 2];

            int a = 0;
            for (byte i = 0; i < Main.maxPlayers; i++)
                if ((i != Player1) && (i != Player2))
                    all[a++] = i;

            return all;
        }

        public static byte[] AllExcept(IEnumerable<byte> Players)
        {
            byte[] all = new byte[Main.maxPlayers - Players.Count()];

            int a = 0;
            for (byte i = 0; i < Main.maxPlayers; i++)
                if (!Players.Contains(i))
                    all[a++] = i;

            return all;
        }

        #endregion

        #region CheckCanSee

        public static bool CheckCanSee(byte Seer, byte Seen) =>
            ((Seer == Seen) || (Seer == byte.MaxValue)
            || (Seen == byte.MaxValue) || CanSee[Seer, Seen]);

        #endregion
        #region WhoCanSee

        public static IEnumerable<byte> WhoCanSee(byte Player)
        {
            if ((Player == byte.MaxValue) || (TShock.Players[Player]?.Active != true))
                yield break;
            for (byte i = 0; i < Main.maxPlayers; i++)
                if (CanSee[i, Player] && (TShock.Players[i]?.Active == true))
                    yield return i;
        }

        #endregion
        #region SeenBy

        public static IEnumerable<byte> SeenBy(byte Player)
        {
            if ((Player == byte.MaxValue) || (TShock.Players[Player]?.Active != true))
                yield break;
            for (byte i = 0; i < Main.maxPlayers; i++)
                if (CanSee[Player, i] && (TShock.Players[i]?.Active == true))
                    yield return i;
        }

        #endregion
        #region SetCanSee

        public static void SetCanSee(byte Observer, bool CanSee, byte Target, bool Send = true)
        {
            if ((Observer == Target)
                    || (Observer == byte.MaxValue)
                    || (Target == byte.MaxValue)
                    || (GPAPI.CanSee[Observer, Target] == CanSee))
                return;
            if (CanSee)
            {
                GPAPI.CanSee[Observer, Target] = true;

                if (Send && (TShock.Players[Observer]?.Active == true)
                         && (TShock.Players[Target]?.Active == true))
                {
                    NetMessage.SendData(14, Observer, -1, null, Target, 1);
                    NetMessage.SendData(4, Observer, -1, null, Target);
                    NetMessage.SendData(13, Observer, -1, null, Target);
                    NetMessage.SendData(16, Observer, -1, null, Target);
                    NetMessage.SendData(30, Observer, -1, null, Target);
                    NetMessage.SendData(45, Observer, -1, null, Target);
                    NetMessage.SendData(42, Observer, -1, null, Target);
                    NetMessage.SendData(50, Observer, -1, null, Target);

                    Player plr = Main.player[Target];

                    for (int index = 0; index < 59; ++index)
                        NetMessage.SendData(5, Observer, -1, null, Target, index,
                            plr.inventory[index].prefix);

                    for (int index = 0; index < plr.armor.Length; ++index)
                        NetMessage.SendData(5, Observer, -1, null, Target, 59 + index,
                            plr.armor[index].prefix);

                    for (int index = 0; index < plr.dye.Length; ++index)
                        NetMessage.SendData(5, Observer, -1, null, Target,
                            59 + plr.armor.Length + index, plr.dye[index].prefix);

                    for (int index = 0; index < plr.miscEquips.Length; ++index)
                        NetMessage.SendData(5, Observer, -1, null, Target,
                            59 + plr.armor.Length + plr.dye.Length + index,
                                plr.miscEquips[index].prefix);

                    for (int index = 0; index < plr.miscDyes.Length; ++index)
                        NetMessage.SendData(5, Observer, -1, null, Target,
                            59 + plr.armor.Length + plr.dye.Length
                            + plr.miscEquips.Length + index, plr.miscDyes[index].prefix);
                }
            }
            else
            {
                if (Send && (TShock.Players[Observer]?.Active == true)
                         && (TShock.Players[Target]?.Active == true))
                    NetMessage.SendData(14, Observer, -1, null, Target, 0);

                GPAPI.CanSee[Observer, Target] = false;
            }
        }

        public static void SetCanSee(byte Observer, bool CanSee,
            IEnumerable<byte> Targets, bool Send = true)
        {
            if (Targets == null)
                throw new ArgumentNullException(nameof(Targets));
            foreach (byte target in Targets)
                SetCanSee(Observer, CanSee, target, Send);
        }

        public static void SetCanSee(IEnumerable<byte> Observers,
            bool CanSee, byte Target, bool Send = true)
        {
            if (Observers == null)
                throw new ArgumentNullException(nameof(Observers));
            foreach (byte observer in Observers)
                SetCanSee(observer, CanSee, Target, Send);
        }

        public static void SetCanSee(IEnumerable<byte> Observers,
            bool CanSee, IEnumerable<byte> Targets, bool Send = true)
        {
            if (Observers == null)
                throw new ArgumentNullException(nameof(Observers));
            if (Targets == null)
                throw new ArgumentNullException(nameof(Targets));
            foreach (byte target in Targets)
                foreach (byte observer in Observers)
                    SetCanSee(observer, CanSee, target, Send);
        }

        #endregion
    }
}
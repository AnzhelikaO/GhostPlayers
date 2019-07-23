﻿#region Using
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using TShockAPI;
#endregion
namespace GhostPlayers
{
    public static class GhostPlayersAPI
    {
        internal static bool[,] CanSee = new bool[Main.maxPlayers, Main.maxPlayers];
        public static readonly byte[] All = Enumerable.Range(0, Main.maxPlayers)
                                                      .Select(i => (byte)i)
                                                      .ToArray();

        #region CheckIfAPlayerCanSeeAnotherPlayer

        public static bool CheckIfAPlayerCanSeeAnotherPlayer(byte Seer, byte Seen) =>
            ((Seer == Seen) || (Seer == byte.MaxValue)
            || (Seen == byte.MaxValue) || CanSee[Seer, Seen]);

        #endregion
        #region GetPlayersWhoCanSeeThisPlayer

        public static IEnumerable<byte> GetPlayersWhoCanSeeThisPlayer(byte Player)
        {
            if ((Player == byte.MaxValue) || (TShock.Players[Player]?.Active != true))
                yield break;
            for (byte i = 0; i < Main.maxPlayers; i++)
                if (CanSee[i, Player] && (TShock.Players[i]?.Active == true))
                    yield return i;
        }

        #endregion
        #region GetPlayersWhoCanBeSeenByThisPlayer

        public static IEnumerable<byte> GetPlayersWhoCanBeSeenByThisPlayer(byte Player)
        {
            if ((Player == byte.MaxValue) || (TShock.Players[Player]?.Active != true))
                yield break;
            for (byte i = 0; i < Main.maxPlayers; i++)
                if (CanSee[Player, i] && (TShock.Players[i]?.Active == true))
                    yield return i;
        }

        #endregion
        #region SetCanSee

        public static void SetCanSee(byte Observer, bool CanSee, byte Target)
        {
            if ((Observer == Target)
                    || (Observer == byte.MaxValue)
                    || (Target == byte.MaxValue)
                    || (TShock.Players[Observer]?.Active != true)
                    || (TShock.Players[Target]?.Active != true))
                return;
            if (CanSee)
            {
                GhostPlayersAPI.CanSee[Observer, Target] = true;

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
            else
            {
                NetMessage.SendData(14, Observer, -1, null, Target, 0);
                GhostPlayersAPI.CanSee[Observer, Target] = false;
            }
        }

        public static void SetCanSee(byte Observer, bool CanSee, IEnumerable<byte> Targets)
        {
            if (Targets == null)
                throw new ArgumentNullException(nameof(Targets));
            foreach (byte target in Targets)
                SetCanSee(Observer, CanSee, target);
        }

        public static void SetCanSee(IEnumerable<byte> Observers, bool CanSee, byte Target)
        {
            if (Observers == null)
                throw new ArgumentNullException(nameof(Observers));
            foreach (byte observer in Observers)
                SetCanSee(observer, CanSee, Target);
        }

        public static void SetCanSee(IEnumerable<byte> Observers,
            bool CanSee, IEnumerable<byte> Targets)
        {
            if (Observers == null)
                throw new ArgumentNullException(nameof(Observers));
            if (Targets == null)
                throw new ArgumentNullException(nameof(Targets));
            foreach (byte target in Targets)
                foreach (byte observer in Observers)
                    SetCanSee(observer, CanSee, target);
        }

        #endregion
    }
}
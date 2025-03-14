using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;
using TerRoguelike.Packets;
using TerRoguelike.TerPlayer;
using TerRoguelike.Utilities;
using System.Reflection;
using System.IO;
using static TerRoguelike.Packets.TerRoguelikePacket;

namespace TerRoguelike.Systems
{
    public class NetcodeSystem : ModSystem
    {
        private static TerRoguelikePacket[] _PacketRegistry;
        public override void OnModLoad()
        {
            _PacketRegistry = new TerRoguelikePacket[256];

            TerRoguelikeUtils.IterateEveryModsTypes<TerRoguelikePacket>(action: type =>
            {
                try
                {
                    if (Activator.CreateInstance(type) is not TerRoguelikePacket packetHandler)
                        return;

                    var msgType = (int)packetHandler.MessageType;
                    var existingHandler = _PacketRegistry[msgType];
                    if (existingHandler != null)
                    {
                        TerRoguelike.Instance.Logger.Error($"Packet instance has already registered by other type!" +
                            $" [Failed On: '{type.FullName}'" +
                            $" Current Owner: '{existingHandler.GetType().FullName}'," +
                            $" msgTypeToRegister: '{msgType}']");
                        return;
                    }

                    _PacketRegistry[(int)packetHandler.MessageType] = packetHandler;

                    var instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
                    if (instanceProperty is not null)
                    {
                        if (instanceProperty.PropertyType.IsAssignableFrom(type))
                        {
                            instanceProperty.SetValue(null, packetHandler);
                            packetHandler._Prop_Static_Instance = instanceProperty; // We saving this for Unload Steps
                        }
                        else
                        {
                            TerRoguelike.Instance.Logger.Error($"Packet instance's 'Instance' property is not asssignable with given type!" +
                                $" [Failed On: '{type.FullName}']");
                        }
                    }
                }
                catch (Exception e)
                {
                    TerRoguelike.Instance.Logger.Error($"Exception was thrown while loading for Packets! {e}");
                    return;
                }
            });
        }

        public override void OnModUnload()
        {
            if (_PacketRegistry is not null)
            {
                foreach (var packetHandler in _PacketRegistry)
                {
                    if (packetHandler is null)
                        continue;

                    packetHandler._Prop_Static_Instance?.SetValue(null, null);
                    packetHandler._Prop_Static_Instance = null;
                }

                _PacketRegistry = null;
            }
        }

        public static void HandlePacket(Mod mod, BinaryReader reader, int whoAmI)
        {
            try
            {
                PacketType msgType = (PacketType)reader.ReadByte();
                var packetHandler = _PacketRegistry[(byte)msgType];
                if (packetHandler is not null)
                {
                    packetHandler.HandlePacket(in reader, whoAmI);
                }
                else
                {
                    //
                    // Default case: with no idea how long the packet is, we can't safely read data.
                    // Throw an exception now instead of allowing the network stream to corrupt.
                    //

                    TerRoguelike.Instance.Logger.Error($"Failed to parse TerRoguelike packet: No TerRoguelike packet exists with ID {msgType}.");
                    throw new Exception("Failed to parse TerRoguelike packet: Invalid TerRoguelike packet ID.");
                }
            }
            catch (Exception e)
            {
                if (e is EndOfStreamException eose)
                    TerRoguelike.Instance.Logger.Error("Failed to parse TerRoguelike packet: Packet was too short, missing data, or otherwise corrupt.", eose);
                else if (e is ObjectDisposedException ode)
                    TerRoguelike.Instance.Logger.Error("Failed to parse TerRoguelike packet: Packet reader disposed or destroyed.", ode);
                else if (e is IOException ioe)
                    TerRoguelike.Instance.Logger.Error("Failed to parse TerRoguelike packet: An unknown I/O error occurred.", ioe);
                else
                    throw; // this either will crash the game or be caught by TML's packet policing
            }
        }
    }
}

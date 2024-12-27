﻿using System;
using System.Drawing;
using Core;
using Microsoft.VisualBasic.CompilerServices;
using Mirage.Sharp.Asfw;

namespace Client
{

    static class Resource
    {

        #region Database

        public static void ClearResource(int index)
        {
            Core.Type.Resource[index] = default;
            Core.Type.Resource[index].Name = "";
            GameState.Resource_Loaded[index] = 0;
        }

        public static void ClearResources()
        {
            int i;

            for (i = 0; i <= Constant.MAX_RESOURCES - 1; i++)
                ClearResource(i);

        }

        public static void StreamResource(int resourceNum)
        {
            if (Conversions.ToBoolean(Operators.OrObject(resourceNum > 0 & string.IsNullOrEmpty(Core.Type.Resource[resourceNum].Name), Operators.ConditionalCompareObjectEqual(GameState.Resource_Loaded[resourceNum], 0, false))))
            {
                GameState.Resource_Loaded[resourceNum] = 1;
                SendRequestResource(resourceNum);
            }
        }

        #endregion

        #region Incoming Packets

        public static void Packet_MapResource(ref byte[] data)
        {
            int i;
            var buffer = new ByteStream(data);
            GameState.ResourceIndex = buffer.ReadInt32();
            GameState.ResourcesInit = Conversions.ToBoolean(0);

            if (GameState.ResourceIndex > 0)
            {
                Array.Resize(ref Core.Type.MapResource, GameState.ResourceIndex + 1);

                var loopTo = GameState.ResourceIndex;
                for (i = 0; i <= loopTo; i++)
                {
                    Core.Type.MyMapResource[i].State = buffer.ReadByte();
                    Core.Type.MyMapResource[i].X = buffer.ReadInt32();
                    Core.Type.MyMapResource[i].Y = buffer.ReadInt32();
                }

                GameState.ResourcesInit = Conversions.ToBoolean(1);
            }

            buffer.Dispose();
        }

        public static void Packet_UpdateResource(ref byte[] data)
        {
            int resourceNum;
            var buffer = new ByteStream(data);
            resourceNum = buffer.ReadInt32();

            Core.Type.Resource[resourceNum].Animation = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].EmptyMessage = buffer.ReadString();
            Core.Type.Resource[resourceNum].ExhaustedImage = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].Health = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].ExpReward = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].ItemReward = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].Name = buffer.ReadString();
            Core.Type.Resource[resourceNum].ResourceImage = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].ResourceType = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].RespawnTime = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].SuccessMessage = buffer.ReadString();
            Core.Type.Resource[resourceNum].LvlRequired = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].ToolRequired = buffer.ReadInt32();
            Core.Type.Resource[resourceNum].Walkthrough = Conversions.ToBoolean(buffer.ReadInt32());

            buffer.Dispose();
        }

        #endregion

        #region Outgoing Packets

        public static void SendRequestResource(int resourceNum)
        {
            var buffer = new ByteStream(4);

            buffer.WriteInt32((int)Packets.ClientPackets.CRequestResource);

            buffer.WriteInt32(resourceNum);
            NetworkConfig.Socket.SendData(buffer.Data, buffer.Head);
            buffer.Dispose();
        }

        #endregion

        #region Drawing

        internal static void DrawResource(int resource, int dx, int dy, Rectangle rec)
        {
            int x;
            int y;
            int width;
            int height;

            if (resource < 1 | resource > GameState.NumResources)
                return;

            x = GameLogic.ConvertMapX(dx);
            y = GameLogic.ConvertMapY(dy);
            width = rec.Right - rec.Left;
            height = rec.Bottom - rec.Top;

            if (rec.Width < 0 | rec.Height < 0)
                return;

            string argpath = System.IO.Path.Combine(Core.Path.Resources, resource.ToString());
            GameClient.RenderTexture(ref argpath, x, y, rec.X, rec.Y, rec.Width, rec.Height, rec.Width, rec.Height);
        }

        internal static void DrawMapResource(int resourceNum)
        {
            int resourceMaster;

            int resourceState;
            var resourceSprite = default(int);
            var rec = default(Rectangle);
            int x;
            int y;

            if (GameState.GettingMap)
                return;
            if (Conversions.ToInteger(GameState.MapData) == 0)
                return;

            if (Core.Type.MyMapResource[resourceNum].X > Core.Type.MyMap.MaxX | Core.Type.MyMapResource[resourceNum].Y > Core.Type.MyMap.MaxY)
                return;

            // Get the Resource type
            resourceMaster = Core.Type.MyMap.Tile[Core.Type.MyMapResource[resourceNum].X, Core.Type.MyMapResource[resourceNum].Y].Data1;

            if (resourceMaster == 0)
                return;

            if (Core.Type.Resource[resourceMaster].ResourceImage == 0)
                return;

            StreamResource(resourceMaster);

            // Get the Resource state
            resourceState = Core.Type.MyMapResource[resourceNum].State;

            if (resourceState == 0) // normal
            {
                resourceSprite = Core.Type.Resource[resourceMaster].ResourceImage;
            }
            else if (resourceState == 1) // used
            {
                resourceSprite = Core.Type.Resource[resourceMaster].ExhaustedImage;
            }

            // src rect
            rec.Y = 0;
            rec.Height = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Resources, resourceSprite.ToString())).Height;
            rec.X = 0;
            rec.Width = GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Resources, resourceSprite.ToString())).Width;

            // Set base x + y, then the offset due to size
            x = (int)Math.Round(Core.Type.MyMapResource[resourceNum].X * GameState.PicX - GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Resources, resourceSprite.ToString())).Width / 2d + 16d);
            y = Core.Type.MyMapResource[resourceNum].Y * GameState.PicY - GameClient.GetGfxInfo(System.IO.Path.Combine(Core.Path.Resources, resourceSprite.ToString())).Height + 32;

            DrawResource(resourceSprite, x, y, rec);
        }

        #endregion

    }
}
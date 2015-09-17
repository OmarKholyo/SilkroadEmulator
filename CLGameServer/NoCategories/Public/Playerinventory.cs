﻿using System;
using CLGameServer.Client;
using CLFramework;
namespace CLGameServer
{
    public partial class PlayerMgr
    {
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Update Player Slot / Remove item
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        void HandleUpdateSlot(byte slot, ObjData.slotItem item, int packet)
        {
            try
            {
                item.Amount--;
                client.Send(Packet.Player_HandleUpdateSlot(slot, (ushort)item.Amount, packet));
                if (item.Amount > 0)
                {
                    DB.query("UPDATE char_items SET quantity='" + Math.Abs(item.Amount) + "' WHERE owner='" + Character.Information.CharacterID + "' AND itemnumber='item" + item.Slot + "' AND id='" + item.dbID + "'");
                }
                else
                {
                    DB.query("delete from char_items where id='" + item.dbID + "'");
                }
                //Need to be defined per item type (potion or private item)
                Send(Packet.Player_HandleEffect(Character.Information.UniqueID, item.ID));
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        void HandleUpdateSlotChange(byte slot, ObjData.slotItem item, int packet)
        {
            DB.query("UPDATE char_items SET itemid='" + packet + "' WHERE owner='" + Character.Information.CharacterID + "' AND itemnumber='item" + item.Slot + "' AND id='" + item.dbID + "'");
            //Need to be defined per item type (potion or private item)
            Send(Packet.Player_HandleEffect(Character.Information.UniqueID, item.ID));
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Update Player Slot / Do not remove item
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        void HandleUpdateSlotn(byte slot, ObjData.slotItem item, int packet)
        {
            try
            {
                client.Send(Packet.Player_HandleUpdateSlot(slot, (ushort)item.Amount, packet));
                Send(Packet.Player_HandleEffect(Character.Information.UniqueID, item.ID));
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Check Available Potion Slot
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static byte Getfreepotslot(byte[] r)
        {
            try
            {
                for (byte b = 0; b < r.Length; b++)
                    if (r[b] == 0) return b;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            return 255;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Item Storage Box
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void ItemStorageBox()
        {
            client.Send(Packet.StorageBox());
        }
        public void ItemStorageBoxLog()
        {
            client.Send(Packet.StorageBoxLog());
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Repair Items
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        void HandleRepair(byte slot, int itemid)
        {
            try
            {
                //Here we use 2nd check for item durability to be sure the item repair is needed.
                //Check max durability on items < Need to check later for stats
                double checkdurability = ObjData.Manager.ItemBase[itemid].Defans.Durability;
                //Load our items
                DB ms = new DB("SELECT * FROM char_items WHERE owner='" + Character.Information.CharacterID + "'");
                using (System.Data.SqlClient.SqlDataReader reader = ms.Read())
                {
                    while (reader.Read())
                    {
                        //Read durability from db
                        int currentdurability = reader.GetInt32(7);
                        //If durability is lower then item durability
                        if (currentdurability < checkdurability)
                        {
                            //Send repair packet to client
                            client.Send(Packet.RepairItems(slot, checkdurability));
                            //Update database information
                            DB.query("UPDATE char_items SET durability='" + checkdurability + "' WHERE id='" + itemid + "' AND owner='" + Character.Information.CharacterID + "' AND storagetype='0'");
                        }
                    }
                }
                ms.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Repair item error {0}", ex);
                Log.Exception(ex);
            }
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Inventory expansion
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        bool HandleInventoryExp(int ItemID)
        {
            try
            {
                //If item has been used before
                if (Character.Information.ExpandedStorage == 1)
                {
                    client.Send(Packet.Message(OperationCode.SERVER_PLAYER_HANDLE_UPDATE_SLOT,Messages.UIIT_STT_STORAGE_EXPANSION_USE_ERORR));
                    return false;
                }
                //Continue to update inventory
                else
                {
                    Character.Information.ExpandedStorage = 1;
                    Character.Information.Slots += 32;
                    DB.query("UPDATE character SET Slots='" + Character.Information.Slots + "',Storage_Expanded='1' WHERE Name='" + Character.Information.Name + "'");
                    client.Send(Packet.AddInventorySlots(Character.Information.Slots));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
            return false;
        }
    }
}

﻿using System;
using System.Threading;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Diagnostics;
using CLGameServer.Client;
using CLFramework;
namespace CLGameServer
{
    public partial class PlayerMgr
    {
        public _time Timer;

        public struct _time
        {
            public Timer pTimer;
            public Timer Logout;
            public Timer Movement;
            public Timer StopMovement;
            public Timer SkyDrome;
            public Timer Attack;
            public Timer[] Buff;
            public Timer Casting;
            public Timer SkillCasting;
            public Timer[] Potion;
            public Timer sWait;
            public Timer Pvp;
            public Timer Berserker;
            public Timer Scroll;
            public Timer Sitting;
            public Timer Skillup;
            public Timer Masteryup;
            public Timer Movementskill;
            public Timer RepairTimer;
            public Timer Pickup;
            public Timer[] EffectTimer;
            public Timer AlchemyTimer;
			public Timer NotAttackAbleTimer;
            public Stopwatch SpawnWatch;
            public Timer Jobequip;
            public Timer NormalAttack;
        }

        void OpenTimer()
        {
            Timer.Buff = new Timer[2000];
            Timer.Potion = new Timer[20];

            Timer.EffectTimer = new Timer[20];
            Timer.SpawnWatch = new Stopwatch();
            /*Watch for spawn check limit*/
            Timer.SpawnWatch.Start();
        }
        void StartSitDownTimer()
        {
            if (Timer.Sitting != null)
            {
                Timer.Sitting.Dispose();
                Timer.Sitting = null;
            }
            else
            {
                Timer.Sitting = new Timer(new TimerCallback(SitDownTimerCB), 0, 1700, 0);
            }
        }
        void SitDownTimerCB(object ca)
        {
            Character.State.Standing = false;
            Timer.Sitting.Dispose();
            Timer.Sitting = null;
        }
        //##############################################################################
        // Start timers voids
        //##############################################################################
        public void StartPvpTimer(int time)
        {
            //Need to add checks
            PacketReader reader = new PacketReader(PacketInformation.buffer);
            byte pvptype = reader.Byte();

            if (Timer.Pvp != null) Timer.Pvp.Dispose();
            Timer.Pvp = new Timer(new TimerCallback(Player_Pvp_CallBack), 0, time, 0);
            Send(Packet.PvpSystemWait(Character.Information.UniqueID));
            Character.Information.PvpWait = true;
            Character.Information.Pvptype = pvptype;
        }
        void StartSpeedPotTimer(int time)
        {
            if (Timer.Scroll != null) Timer.Scroll.Dispose();
            Timer.Scroll = new Timer(new TimerCallback(Player_Scroll_CallBack), 0, time, 0);
        }
        void StartPotionTimer(int time, object e, ushort i)
        {
            Timer.Potion[i] = new Timer(new TimerCallback(Player_Potion_CallBack), e, 0, time);
        }
        void StartSkillCastingTimer(int time, object list)
        {
            if (Timer.SkillCasting != null) Timer.SkillCasting.Dispose();
            Timer.SkillCasting = new Timer(new TimerCallback(Player_SkillCasting_CallBack), list, time, 0);
        }
        void StartBerserkerTimer(int time)
        {
            if (Timer.Berserker != null) Timer.Berserker.Dispose();
            Timer.Berserker = new Timer(new TimerCallback(Player_Berserker_CallBack), 0, time, 0);
        }
        void StartWaitingTimer(int time)
        {
            if (Timer.Logout != null) Timer.Logout.Dispose();
            Timer.Logout = new Timer(new TimerCallback(Player_Wait_CallBack), 0, time, 0);
        }
        void StartsWaitTimer(int time, WorldMgr.targetObject[] t, int[,] p_dmg, byte[] status)
        {
            if (Timer.sWait != null) { Timer.sWait.Dispose(); }
            Timer.sWait = new Timer(new TimerCallback(Player_sWait_Attack_CallBack), new object[] { t, p_dmg, status }, time, 0);
        }
        void StartMovementTimer(int perTime)
        {
            if(Timer.Movement != null)Timer.Movement.Dispose();
            Timer.Movement = new Timer(new TimerCallback(Player_Movement), 0, 0, perTime);
        }
        void StopMovement(int perTime)
        {
            if (Timer.StopMovement != null) Timer.StopMovement.Dispose();
            Timer.StopMovement = new Timer(new TimerCallback(Stop_Movement), 0, 0, perTime);
        }
        void StartSkyDromeTimer(int perTime)
        {
            if (Timer.Movement != null) Timer.Movement.Dispose();
            Timer.Movement = new Timer(new TimerCallback(Player_SkyDrome), 0, 0, perTime);
        }
        void StartBuffTimer(int time, byte b_index)
        {
            if (Timer.Buff[b_index] != null) Timer.Buff[b_index].Dispose();
            Timer.Buff[b_index] = new Timer(new TimerCallback(Player_Buff_CallBack), b_index, time, 0);
        }
        void StartEventTimer(int time)
        {
            if (Timer.Sitting != null) Timer.Sitting.Dispose();
            Timer.Sitting = new Timer(new TimerCallback(Event_callback), 0, 5000, 0);
        }
        void StartCastingTimer(int time, object list)
        {
            if (Timer.Casting != null) Timer.Casting.Dispose();
            Timer.Casting = new Timer(new TimerCallback(Player_Casting_CallBack), list, time, 0);
        }
        void StartBuffWait(int time)
        {
            if (Timer.Casting != null) Timer.Casting.Dispose();
            Timer.Casting = new Timer(new TimerCallback(Player_Casting_CallBack_Check), 0, time, 0);
        }
        void StartAttackTimer()
        {
            int time = 0;
            if (Character.Information.Item.wID != 0)
            {
                switch (ObjData.Manager.ItemBase[Character.Information.Item.wID].TypeID4)
                {
                    //Chinese base skills
                    case 2:                 //One handed sword
                    case 3:
                        time = ObjData.Manager.SkillBase[2].CastingTime;
                        break;
                    case 4:                 //Spear attack + glavie
                    case 5:
                        time = ObjData.Manager.SkillBase[40].CastingTime;
                        break;
                    case 6:                 //Bow attack
                        time = ObjData.Manager.SkillBase[70].CastingTime;
                        break;
                    //Europe Base skills
                    case 7:
                        time = ObjData.Manager.SkillBase[7127].CastingTime;
                        break;
                    case 8:
                        time = ObjData.Manager.SkillBase[7128].CastingTime;
                        break;
                    case 9:
                        time = ObjData.Manager.SkillBase[7129].CastingTime;
                        break;
                    case 10:
                        time = ObjData.Manager.SkillBase[9069].CastingTime;
                        break;
                    case 11:
                        time = ObjData.Manager.SkillBase[8454].CastingTime;
                        break;
                    case 12:
                        time = ObjData.Manager.SkillBase[7909].CastingTime;
                        break;
                    case 13:
                        time = ObjData.Manager.SkillBase[7910].CastingTime;
                        break;
                    case 14:
                        time = ObjData.Manager.SkillBase[7606].CastingTime;
                        break;
                    case 15:
                        time = ObjData.Manager.SkillBase[9970].CastingTime;
                        break;
                    case 16:
                        time = ObjData.Manager.SkillBase[Character.Action.UsingSkillID].ID;
                        break;
                }
            }
            else
            {

            }
            if (Timer.Attack != null) Timer.Attack.Dispose();
            Timer.Attack = new Timer(new TimerCallback(Player_Attack_CallBack), 0, 0, time + Rnd.Next(100, 400));
        }
        void StartNormalAttTimer(int time)
        {
            if (Timer.NormalAttack != null) Timer.Attack.Dispose();
            Timer.NormalAttack = new Timer(new TimerCallback(Player_NormalAttack_Callback), 0, 0, time);
        }
        void StartPickupTimer(int time)
        {
            if (Timer.Pickup != null) Timer.Pickup.Dispose();
            Timer.Pickup = new Timer(new TimerCallback(Player_Pickup_CallBack), 0, 0, time);
        }
        void StopPickupTimer()
        {
            if (Timer.Pickup != null)
            {
                Timer.Pickup.Dispose();
            }
        }
        void StartScrollTimer(int time)
        {
            if (Timer.Scroll != null) Timer.Scroll.Dispose();
            Timer.Scroll = new Timer(new TimerCallback(Player_Scroll_CallBack), 0, time, 0);
        }
        void StartJobEquipTimer(int time)
        {
            if (Timer.Jobequip != null) Timer.Jobequip.Dispose();
            Timer.Jobequip = new Timer(new TimerCallback(Player_Jobequip_CallBack), 0, time, 0);
        }
        public void StartEffectTimer(int time, byte e_index)
        {
            if (Timer.EffectTimer[e_index] != null) Timer.EffectTimer[e_index].Dispose();
            Timer.EffectTimer[e_index] = new Timer(new TimerCallback(Player_Effect_CallBack), e_index, time, 0);
        }

        void MovementSkillTimer(int time)
        {
        	if (Timer.Movementskill != null)
        		Timer.Movementskill.Dispose();
        	Timer.Movementskill = new Timer(new TimerCallback(Player_MovementSkill_CallBack), 0, time, 0);

  		}
        void NotAttackableTimer(int fortime)
        {
            if (Timer.NotAttackAbleTimer != null)
                Timer.NotAttackAbleTimer.Dispose();
            Timer.NotAttackAbleTimer = new Timer(new TimerCallback(NotAttackableCallback), 0, fortime, 0);
        }
        void NotAttackableCallback(object e = null)
        {
            Character.State.SafeState = false;
            client.Send(Packet.StatePack(Character.Information.UniqueID, 0x04, 0x00, false));
        }
        void RepairTimer(int time)
        {
            if (Timer.RepairTimer != null) Timer.RepairTimer.Dispose();
            Timer.RepairTimer = new Timer(new TimerCallback(Player_Repair_Callback), 0, time, 0);
        }
        void StartPvpTimer(int time, byte pvptype)
        {
            
        }
        //##############################################################################
        // Stop timers voids
        //##############################################################################
        void StopSkyDromeTimer()
        {
            Character.Information.SkyDroming = false;

            if (Timer.SkyDrome != null)
            {
                Timer.SkyDrome.Dispose();
                Timer.SkyDrome = null;
            }
        }
        public void StopSkillTimer()
        {
            try
            {
                Character.Action.sSira = 0;
                Character.Action.sAttack = false;
                Character.Action.sCasting = false;
                if (Timer.sWait != null) Timer.sWait.Dispose();
            }
            catch (Exception)
            {
            }
        }
        public void StopPvpTimer()
        {
            try
            {
                if (Timer.Pvp != null)
                {
                    Timer.Pvp = null;
                    Timer.Pvp.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }
        void StopEffectTimer(byte e_index)
        {
            if (Timer.EffectTimer[e_index] != null)
            {
                Timer.EffectTimer[e_index].Dispose();
                Timer.EffectTimer[e_index] = null;
            }

            if (Character.Action.DeBuff.Effect.EffectImpactTimer[e_index] != null)
            {
                Character.Action.DeBuff.Effect.EffectImpactTimer[e_index].Dispose();
                Character.Action.DeBuff.Effect.EffectImpactTimer[e_index] = null;
            }
        }
        void StopBerserkTimer()
        {
            if (Timer.Berserker != null)
            {
                Player_Berserk_Down();
                Timer.Berserker.Dispose();
                Timer.Berserker = null;
            }
        }
        public void StopScrollTimer()
        {
            try
            {
                if(Timer.Scroll != null)
                    Timer.Scroll.Dispose();

                Timer.Scroll = null;

                Send(Packet.StatePack(Character.Information.UniqueID, 0x0B, 0x00, false));
                Character.Information.Scroll = false;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        public void StopAttackTimer()
        {
            try
            {
                if (Timer.Attack != null)
                {
                    client.Send(Packet.ActionState(2, 0));
                    Timer.Attack.Dispose();
                    Timer.Attack = null;
                    Character.Action.nAttack = false;
                    Character.Action.Object = null;
                    Character.Action.Target = -1;
                    Console.WriteLine("Attack Durduruldu");
                }
                
            }
            catch (Exception ex)
            {
                //Console.WriteLine("Error::StopAttackTimer({0})", Character.Information.Name);
                Log.Exception(ex);
            }
        }
        //##############################################################################
        // Call backs
        //##############################################################################
        void Player_SkyDrome(object e)
        {
            try
            {
                double distance = Character.Speed.RunSpeed / 10;
                Character.Position.x += (float)(distance * Math.Cos((Math.PI / 180) * Character.Information.Angle));
                Character.Position.y += (float)(distance * Math.Sin((Math.PI / 180) * Character.Information.Angle));
                Character.Position.z = Formule.GetHeightAt( Character.Position.xSec, Character.Position.ySec, Character.Position.x, Character.Position.y );

                Console.WriteLine("x: {0} y: {1} z: {2} angle: {3}", Character.Position.x, Character.Position.y, Character.Position.z, Character.Information.Angle);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        void Stop_Movement(object e)
        {
            try
            {
                if (Timer.Movement != null)
                {
                    Timer.Movement.Dispose();
                }


            }
            catch (Exception ex)
            {
                Log.Exception("Stop movement error ", ex);
            }
        }
        void Player_Movement(object e)
        {
            try
            {
                if (Character.Position.RecordedTime <= 0)
                {
                    Character.aRound = new bool[10];

                    Timer.Movement.Dispose();

                    Character.Position.Walking = false;

                    if (Character.Action.PickUping) 
                        Player_PickUpItem();

                    Character.Position.z = Character.Position.wZ;

                    if (Timer.SpawnWatch.ElapsedMilliseconds >= 10)
                    {
                        ObjectSpawnCheck();
                        Timer.SpawnWatch.Restart();
                    }

                    Movement_CaveTeleport();
                }
                else
                {
                    if (Character.Action.nAttack)
                    {
                        Character.Position.kX -= (Character.Position.wX * 10) / 100;
                        Character.Position.kY -= (Character.Position.wY * 10) / 100;

                        if (Character.Information.Item.wID != 0)
                        {
                            if (Math.Sqrt(Character.Position.kX * Character.Position.kX + Character.Position.kY * Character.Position.kY) <= ObjData.Manager.ItemBase[Character.Information.Item.wID].ATTACK_DISTANCE)
                            {
                                Character.Position.RecordedTime = 0;
                                //Character.aRound = new bool[10];
                                if (Character.Action.nAttack) ActionAttack();

                                if (Timer.SpawnWatch.ElapsedMilliseconds >= 10)
                                {
                                    ObjectSpawnCheck();
                                    Timer.SpawnWatch.Restart();
                                }

                                Timer.Movement.Dispose();
                                Character.Position.z = Character.Position.wZ;
                                Character.Action.PickUping = false;
                                Character.Position.Walking = false;
                                return;
                            }
                        }
                        else
                        {
                            if (Math.Sqrt(Character.Position.kX * Character.Position.kX + Character.Position.kY * Character.Position.kY) <= 1)
                            {
                                Character.Position.RecordedTime = 0;
                                //Character.aRound = new bool[10];
                                if (Character.Action.nAttack) ActionAttack();

                                if (Timer.SpawnWatch.ElapsedMilliseconds >= 1000)
                                {
                                    ObjectSpawnCheck();
                                    Timer.SpawnWatch.Restart();
                                }

                                Timer.Movement.Dispose();
                                Character.Position.z = Character.Position.wZ;
                                Character.Action.PickUping = false;
                                Character.Position.Walking = false;
                                return;
                            }
                        }
                    }
                    else if (Character.Action.sAttack)
                    {
                        Character.Position.kX -= (Character.Position.wX * 10) / 100;
                        Character.Position.kY -= (Character.Position.wY * 10) / 100;
                        double test = Character.Action.Skill.Distance;
                        if (test == 0)
                            test = ObjData.Manager.ItemBase[Character.Information.Item.wID].ATTACK_DISTANCE;
                        if (Math.Sqrt(Character.Position.kX * Character.Position.kX + Character.Position.kY * Character.Position.kY) < test)
                        {
                            if (Character.Action.sAttack) StartSkill();
                            Character.Position.RecordedTime = 0;
                            Character.aRound = new bool[10];

                            if (Timer.SpawnWatch.ElapsedMilliseconds >= 1000)
                            {
                                ObjectSpawnCheck();
                                Timer.SpawnWatch.Restart();
                            }

                            Timer.Movement.Dispose();
                            Character.Position.z = Character.Position.wZ;
                            Character.Action.PickUping = false;
                            Character.Position.Walking = false;
                            return;
                        }
                    }
                    
                    
                    Character.aRound = new bool[10];
                    Character.Position.x += (Character.Position.wX * 10) / 100;
                    Character.Position.y += (Character.Position.wY * 10) / 100;

                    if (Character.Transport.Right)
                    {
                        Character.Transport.Horse.x = Character.Position.x;
                        Character.Transport.Horse.y = Character.Position.y;
                    }
                    Character.Position.RecordedTime -= (Character.Position.Time * 0.1);

                    if (Timer.SpawnWatch.ElapsedMilliseconds >= 10)
                    {
                        ObjectSpawnCheck();
                        ObjectAttackCheck();
                        Timer.SpawnWatch.Restart();
                    }

                }
            }
            catch (Exception ex)
            {
                Log.Exception("Move call error: ", ex);
                Log.Exception(ex);
            }
        }
        /////////////////////////////////////////////////////////////////////////////////
        // Attack system check (Follow)
        /////////////////////////////////////////////////////////////////////////////////    
        void ObjectAttackCheck()
        {
            try
            {
                for (int i = 0; i < Helpers.Manager.Objects.Count; i++)
                {
                    if (Helpers.Manager.Objects[i] != null && Helpers.Manager.Objects[i].LocalType == 1 && Helpers.Manager.Objects[i].Spawned(Character.Information.UniqueID))
                    {
                        if (Helpers.Manager.Objects[i].UniqueID != 0)
                        {
                            Helpers.Manager.Objects[i].FollowHim(this);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        void Player_Buff_CallBack(object e)
        {
            try
            {
                SkillBuffEnd((byte)e);
                if (Timer.Buff[(byte)e] != null) Timer.Buff[(byte)e].Dispose();
                Character.Action.Buff.Casting = false;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        void Player_Pvp_CallBack(object e)
        {
            try
            {
                Pvpsystem();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        void Event_callback(object e)
        {
            try
            {
                Timer.Sitting.Dispose();
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        void Player_Scroll_CallBack(object e)
        {
            try
            {
                if (Character.Information.Scroll == false) return;

                Character.InGame = false;

                BuffAllClose();

                DeSpawnMe();
                ObjectDeSpawnCheck();
                client.Send(Packet.TeleportOtherStart());

                Teleport_UpdateXYZ(Character.Information.Place);
                client.Send(Packet.TeleportImage(ObjData.Manager.PointBase[Character.Information.Place].xSec, ObjData.Manager.PointBase[Character.Information.Place].ySec));
                Character.Teleport = true;
                Timer.Scroll.Dispose();
                Timer.Scroll = null;
                Character.Information.Scroll = false;
                Character.State.Sitting = false;
                Character.Position.Walking = false;
                
            }

            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        void Player_Jobequip_CallBack(object e)
        { 
            /*
            try
            {
               
                Character.InGame = false;

                BuffAllClose();

                DeSpawnMe();
                ObjectDeSpawnCheck();
                client.Send(Packet.TeleportOtherStart());

                Teleport_UpdateXYZ(Character.Information.Place);
                client.Send(Packet.TeleportImage(ObjData.Manager.PointBase[Character.Information.Place].xSec, ObjData.Manager.PointBase[Character.Information.Place].ySec));
                Character.Teleport = true;
                Timer.Scroll.Dispose();
                Timer.Scroll = null;
                Character.Information.Scroll = false;
                Character.State.Sitting = false;
                Character.Position.Walking = false;

                if (Character.Job.Jobname != "0" && Character.Job.state == 0)
                {
                    //Update database
                    DB.query("UPDATE character_jobs SET job_state='1' WHERE character_name='" + Character.Information.Name + "'");
                    Character.Job.state = 1;
                    return;
                }
                else if (Character.Job.Jobname != "0" && Character.Job.state == 1)
                {
                    //Update database
                    DB.query("UPDATE character_jobs SET job_state='0' WHERE character_name='" + Character.Information.Name + "'");
                    Character.Job.state = 0;
                    return;
                }
            }

            catch (Exception ex)
            {
                Log.Exception(ex);
            
                
            } 
             */
        }
        

        void Player_NormalAttack_Callback(object e)
        {
            Character.Action.normalattack = false;
            Timer.Skillup.Dispose();
        }
        void Player_MovementSkill_CallBack(object e)
        {
            Character.Action.movementskill = false;
            Timer.Movementskill.Dispose();
        }

        void Player_Repair_Callback(object e)
        {
            Character.Action.repair = false;
            if (Timer.RepairTimer != null)
                Timer.RepairTimer.Dispose();

        }
        void Player_sWait_Attack_CallBack(object e)
        {
            try
            {
                object[] es = (object[])e;
                WorldMgr.targetObject[] target = (WorldMgr.targetObject[])es[0];
                int[,] p_dmg = (int[,])es[1];
                byte[] staticstatus = (byte[])es[2];

                for (byte f = 0; f < Character.Action.Skill.Found; f++)
                {
                    if (staticstatus[f] == 4)
                        target[f].Sleep(4);

                    target[f].HP(p_dmg[f, Character.Action.sSira]);
                    if (Character.Action.sSira + 1 == Character.Action.Skill.NumberOfAttack)
                    {
                        target[f].Dispose();
                    }
                }
                Character.Action.sSira++;
                if (Character.Action.sSira == Character.Action.Skill.NumberOfAttack)
                {
                    //StartAttackTimer();
                    StopSkillTimer();
                }
                else
                {
                    if (Character.Action.Skill.NumberOfAttack != 1) 
                        StartsWaitTimer(ObjData.Manager.SkillBase[Character.Action.Skill.SkillID[Character.Action.sSira - 1]].CastingTime, target, p_dmg, staticstatus);
                    
                }
            }
            catch (Exception ex)
            {
                StopSkillTimer();
                Log.Exception(ex);
            }
        }
        void Player_Pvp_Callback(object e)
        {
            try
            {

                if (!Character.Information.PvP && !Character.Stall.Stallactive && !Character.Action.nAttack && !Character.Action.sAttack && !Character.Position.Walking)
                {

                }
                else
                {

                }
            }
            catch (Exception)
            {
            }
        }
        void Player_Casting_CallBack(object e)
        {
            try
            {
                if (Character.Action.Cast)
                {
                    SkillBuffCasting((List<int>)e);
                    Character.Action.Cast = false;
                    Character.Action.Buff.Casting = false;
                    Timer.Casting.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Exception("Skill cast timer error: ", ex);
            }
        }

        void Player_Casting_CallBack_Check(object e)
        {
            Character.Action.Buff.Casting = false;
            if (Timer.Casting != null)
                Timer.Casting.Dispose();
        }
        void Player_Effect_CallBack(object e)
        {
            try
            {
                StopEffectTimer((byte)e);

                WorldMgr.targetObject thisObject = new WorldMgr.targetObject(this, null);

                foreach (KeyValuePair<string, int> p in ObjData.Manager.SkillBase[Character.Action.DeBuff.Effect.SkillID[(byte)e]].Properties1)
                {
                    switch (p.Key)
                    {
                        case "fb":
                            CLGameServer.Effect.DeleteEffect_fb(thisObject, (byte)e);
                            break;
                        case "bu":
                            
                            break;
                        case "fz":
                            CLGameServer.Effect.DeleteEffect_fz(thisObject, (byte)e);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
        void Player_Wait_CallBack(object e)
        {
            try
            {
                if (Character.Information.Quit)
                {
                    //##############################################
                    // checks before logout
                    //##############################################
                    if (Character.Position.Walking)
                    {
                        Character.Position.RecordedTime = 0;
                        Timer.Movement.Dispose();
                        Timer.Movement = null;
                    }
                    //##############################################
                    // checks before logout
                    //##############################################
                    if (Character.Information.CheckParty)
                    {
                        LeaveParty();
                    }
                    //##############################################
                    // checks before logout
                    //##############################################
                    if (Character.Network.Guild.Guildid != 0)
                    {
                        Character.Information.Online = 0;
                        //Send packets to network and spawned players
                        foreach (int member in Character.Network.Guild.Members)
                        {
                            //Make sure the member is there
                            if (member != 0)
                            {
                                //We dont send this info to the invited user.
                                if (member != Character.Information.CharacterID)
                                {
                                    //If the user is not the newly invited member get player info
                                    PlayerMgr tomember = Helpers.GetInformation.GetPlayerMainid(member);
                                    //Send guild update packet
                                    if (tomember != null)
                                    {
                                        tomember.client.Send(Packet.GuildUpdate(Character, 6, Character.Information.CharacterID, 0,0));
                                    }
                                }
                            }
                        }
                        Character.Network.Guild.Members.Remove(Character.Information.CharacterID);
                        Character.Network.Guild.MembersClient.Remove(client);
                    }
                    //##############################################
                    // checks before logout
                    //##############################################
                    if (Character.Transport.Right) Character.Transport.Horse.DeSpawnMe();
                    if (Character.Grabpet.Active) UnSummonPetLogoff(Character.Grabpet.Details.UniqueID);
                    if (Character.Attackpet.Active) UnSummonPetLogoff(Character.Attackpet.Details.UniqueID);
                    if (Character.Network.Exchange.Window) Exchange_Close();
                    //##############################################
                    // checks before logout
                    //##############################################
                    DB ms = new DB("SELECT * FROM friends WHERE owner='" + Character.Information.CharacterID + "'");
                    int count = ms.Count();
                    if (count >= 0)
                    {
                        using (SqlDataReader reader = ms.Read())
                        {
                            while (reader.Read())
                            {
                                int getid = reader.GetInt32(2);
                                PlayerMgr sys = Helpers.GetInformation.GetPlayerid(getid);
                                if (sys != null)
                                {
                                    sys.client.Send(Packet.FriendData(Character.Information.CharacterID, 4, Character.Information.Name, Character, true));
                                }
                            }
                        }
                    }
                    //##############################################
                    // Send packet leave game
                    //##############################################
                    client.Send(Packet.EndLeaveGame());
                    //##############################################
                    // Updated database
                    //##############################################
                    DB.query("UPDATE character SET online='0' WHERE id='" + Character.Information.CharacterID + "'");
                    //##############################################
                    // Remove all remaining parts
                    //##############################################
                    BuffAllClose();
                    DeSpawnMe();
                    SavePlayerPosition();
                    SavePlayerInfo();
                    client.Close();
                    Character.Dispose();
                    Dispose();
                    Character.InGame = false;
                    Disconnect("normal");
                }
                Timer.Logout.Dispose();
            }
            catch (Exception ex)
            {
                Log.Exception("Logout error: ", ex);
            }
        }


        void Player_Attack_CallBack(object e)
        {
            if (Timer.Attack != null)
            {
                ActionNormalAttack();
            }
        }
        void Player_Pickup_CallBack(object e)
        {
            if (Timer.Pickup != null)
            {
                client.Send(Packet.ActionState(1, 1));
                Player_PickUp();
            }
        }
        void Player_Berserker_CallBack(object e)
        {
            StopBerserkTimer();
        }
        void Player_SkillCasting_CallBack(object e)
        {
            MainSkill_Attack((List<int>)e);
            Timer.SkillCasting.Dispose();
        }
        void Player_Potion_CallBack(object e)
        {
            try
            {
                int[] prob = (int[])e;

                if (Character.Information.Item.Potion[prob[2]] == 5 || Character.State.Die)
                {
                    if (Timer.Potion[prob[2]] != null)
                    {
                        Character.Information.Item.Potion[prob[2]] = 0;
                        Timer.Potion[prob[2]].Dispose();
                        Timer.Potion[prob[2]] = null;
                        prob = null;
                    }
                    return;
                }
                if (prob[1] == 1)
                {
                    Character.Information.Item.Potion[prob[2]]++;
                    Character.Stat.SecondHp += prob[0];
                    if (Character.Stat.SecondHp > Character.Stat.Hp) { Character.Stat.SecondHp = Character.Stat.Hp; }
                    UpdateHp();
                }
                if (prob[1] == 2)
                {
                    Character.Information.Item.Potion[prob[2]]++;
                    Character.Stat.SecondMP += prob[0];
                    if (Character.Stat.SecondMP > Character.Stat.Mp) { Character.Stat.SecondMP = Character.Stat.Mp; }
                    UpdateMp();
                }
                if (prob[1] == 3)
                {
                    Character.Information.Item.Potion[prob[2]]++;
                    Character.Stat.SecondHp += prob[0];
                    if (Character.Stat.SecondHp > Character.Stat.Hp) { Character.Stat.SecondHp = Character.Stat.Hp; }
                    UpdateHp();
                }
                if (prob[1] == 4)
                {
                    Character.Information.Item.Potion[prob[2]]++;
                    Character.Stat.SecondMP += prob[0];
                    if (Character.Stat.SecondMP > Character.Stat.Mp) { Character.Stat.SecondMP = Character.Stat.Mp; }
                    UpdateMp();
                }
                if (prob[1] == 5)
                {
                    Character.Information.Item.Potion[prob[2]]++;
                    Character.Stat.SecondHp += prob[0];
                    Character.Stat.SecondMP += prob[0];
                    if (Character.Stat.SecondHp > Character.Stat.Hp)
                    {
                        Character.Stat.SecondHp = Character.Stat.Hp;
                        Character.Stat.SecondMP = Character.Stat.Mp;
                    }

                    UpdateHp();
                    UpdateMp();
                }
                if (prob[1] == 6)
                {
                    Character.Information.Item.Potion[prob[2]]++;
                    Character.Stat.SecondHp += prob[0];
                    Character.Stat.SecondMP += prob[0];
                    if (Character.Stat.SecondHp > Character.Stat.Hp)
                    {
                        Character.Stat.SecondHp = Character.Stat.Hp;
                        Character.Stat.SecondMP = Character.Stat.Mp;
                    }

                    UpdateHp();
                    UpdateMp();
                }
                prob = null;
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
            }
        }
    }
}

using System.Linq;
using System;
using Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk.Model;
using System.Collections.Generic;


namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk
{
    public sealed class MyStrategy : IStrategy
    {
        private static double STRIKE_ANGLE = 1.0D * Math.PI / 180.0D;
        private double netX;
        private double netY;
        private double ЦентрПоляХ;
        double ЦентрПоляY;
        private double УдачноеX;
        private double УдачноеY;
        private double УдачныйРадиусY;
        private double УдачныйРадиусX;
        private double НеУдачныйРадиусY;
        Game g_game;
        World w_orld;
        Hockeyist s_elf;

        public void Move(Hockeyist self, World world, Game game, Move move)
        {
            s_elf = self;
            g_game = game; w_orld = world;
            if (ШайбаУМоейКоманды(self, world))
            {
                ПолучитьУдачноеРасположение(world);

                if (ШайбаУМеня(self, world))
                {

                    if (НахожусьВУдачномМесте(self))
                    {

                        if (УдачныйМомент(self, world))
                        {
                           
                            УдарПоВоротам(self, world, move);
                        }
                        else
                        {
                            Сообщить("не удачный момент");
                            ПопробоватьПасс(self, world, move);
                            
                        }
                    }
                    else
                    {
                        ПопробоватьПасс(self, world, move);
 
                    }
                }
                else
                {
                    ЗанятьУдачнуюПозициюНаПоле(self, world, move);
                }

            }
            else
            {
                ДогнатьШайбу(self, world, move);
            }

        }

        private bool ПопробоватьПасс(Hockeyist self, World world, Move move)
        {
            if (ВрагиБлизко(self, world, self.Radius))
                if (ДатьПасс(self, world, move)) return true;
            ЗанятьУдачнуюПозициюНаПоле(self, world, move);
            return false;
        }

        private bool НахожусьВообщеДалеко(Hockeyist self)
        {
            return Math.Abs(self.X - netX) > Math.Abs(ЦентрПоляХ - netX);
        }

        private bool НахожусьВУдачномМесте(Hockeyist self)
        {
            return (self.X < УдачноеX + УдачныйРадиусX)
                && (self.X > УдачноеX - УдачныйРадиусX)
                && (self.Y < УдачноеY + УдачныйРадиусY)
                && (self.Y > УдачноеY - УдачныйРадиусY)
                ;

        }

        private void ПолучитьУдачноеРасположение(World world)
        {
            Player opponentPlayer = world.GetOpponentPlayer();
            ЦентрПоляХ = world.Width * 0.5D;
            ЦентрПоляY = world.Height * 0.5D;
            УдачноеY = ЦентрПоляY*0.5D;
            if ((s_elf.Y + s_elf.SpeedY * s_elf.Mass * 0.01D) > ЦентрПоляY) УдачноеY = (ЦентрПоляY + world.Height )* 0.5D;
            УдачныйРадиусY = Math.Abs(0.7D * (opponentPlayer.NetBottom - opponentPlayer.NetTop));
            УдачноеX = (ЦентрПоляХ + opponentPlayer.NetFront) * 0.5D;
            УдачныйРадиусX = Math.Abs(0.4D * (opponentPlayer.NetBottom - opponentPlayer.NetTop));
            //УдачноеX = (УдачноеX + ЦентрПоляХ) * 0.5D;
        }

        private void Сообщить(string Строка)
        {
            //if ((w_orld.Tick & 1) == 1)
                System.Console.Out.WriteLine(s_elf.Id.ToString() + " " + Строка + "  " + w_orld.Tick.ToString());
        }

        private bool ДатьПасс(Hockeyist self, World world, Model.Move move)
        {
            move.PassPower = 0.5D;
            Hockeyist ПринимающийИгрок = ИгрокГотовПринять(self, world, move);
            if (ПринимающийИгрок == null) return false;
            move.Turn = self.GetAngleTo(ПринимающийИгрок);
            move.PassAngle = self.GetAngleTo(ПринимающийИгрок);
            move.Action = ActionType.Pass;
            if (ПринимающийИгрок != null) Сообщить("Пассую " + ПринимающийИгрок.Id.ToString());
            return true;
        }

        private Hockeyist ИгрокГотовПринять(Hockeyist self, World world, Model.Move move)
        {

            var ИщемСвоего = from Hockeyist игрок in world.Hockeyists where игрок.Id != self.Id && игрок.IsTeammate && игрок.Type != HockeyistType.Goalie && игрок.State == HockeyistState.Active && НахожусьВУдачномМесте(игрок) && КтоНаЛинииОгня(self, world, игрок.X, игрок.Y) == null select игрок;
            return ИщемСвоего.FirstOrDefault();
        }

        private bool УдачныйМомент(Hockeyist self, World world)
        {
            ПолучитьТочкуУдараПоВоротам(self, world, out netX, out netY);
            return (КтоНаЛинииОгня(self, world, netX, netY) == null);
        }

        private Hockeyist КтоНаЛинииОгня(Hockeyist self, World world, double x, double y)
        {

            var Ищем = from Hockeyist игрок in world.Hockeyists where !игрок.IsTeammate && игрок.Id != self.Id && НаЛинии(self, x, y, игрок, world.Puck.Radius) select игрок;
            Hockeyist ИгрокНаПути = Ищем.FirstOrDefault();
            if (ИгрокНаПути != null) Сообщить(ИгрокНаПути.Id.ToString() + " на пути");
            return ИгрокНаПути;

        }
        private IEnumerable<Hockeyist> КтоНаПути(Hockeyist self, World world, double x, double y, double r)
        {
            return from Hockeyist игрок in world.Hockeyists where !игрок.IsTeammate && self.GetDistanceTo(игрок) < r && НаЛинии(self, x, y, игрок, world.Puck.Radius) select игрок;
        }

        private bool НаЛинии(Hockeyist self, double x, double y, Hockeyist игрок, double p)
        {
            double ty = self.Y + ((игрок.X - x) * (y - self.Y)) / (x - self.X);
            return Math.Abs(ty - игрок.Y) < игрок.Radius + p * 0.5D;
        }



        private void ЗанятьУдачнуюПозициюНаПоле(Hockeyist self, World world, Model.Move move)
        {
            move.Action = ActionType.TakePuck;
            if (НахожусьВУдачномМесте(self))
            {
                double angleToNet = self.GetAngleTo(netX, netY);
                move.Turn = angleToNet;
                move.SpeedUp = 0.0D;
                if (!ШайбаУМеня(self, world))
                {
                    move.SpeedUp = -0.5D;
                    if (ВрагиБлизко(self, world, g_game.StickLength*0.5D)) { 
                        move.Action = ActionType.Strike;
                        move.SpeedUp = 0.0D;
                        Сообщить("Драка"); }

                }
                else {
                    if (ВрагиБлизко(self, world, self.Radius)) move.SpeedUp = -1.0D;
                
                
                }
                

            }
            else
            {


                if (ШайбаУМеня(self, world)) {
                    if (УбежалОтВрагов(self,world,move))
                        ИдтиКЦели(self, move, УдачноеX, УдачноеY);
                }
                else
                {
                    ИдтиКЦели(self, move, УдачноеX, УдачноеY);
                }
                
            }
        }

 

        private bool УбежалОтВрагов(Hockeyist self, World world, Model.Move move)
        {
            double r = 2.0D * world.Puck.Radius + 4.0D * g_game.StickLength;
            
            var ВрагиКругом = from Hockeyist игрок in world.Hockeyists where !игрок.IsTeammate 
                                  && self.GetDistanceTo(игрок) < (r)
                                  && Math.Abs(self.GetAngleTo(игрок) + игрок.Angle) < STRIKE_ANGLE * Math.PI
                              select игрок;
            if (ВрагиКругом.Count() == 0) return true;
            double x=0.0D, y=0.0D;
            foreach (var игрок in ВрагиКругом) {
                x += (игрок.X + игрок.SpeedX);
                x *= 0.5D;
                y += (игрок.SpeedY + игрок.Y);
                y *= 0.5D;
            }
            //ИдтиКЦели(self, move, x, y);
            double Fangle = -self.GetAngleTo(x, y);
            ИдтиТуда(move, Fangle);
            Сообщить("Убегаю");
            return false;
        }

        private void ИдтиКЦели(Hockeyist self, Model.Move move, double УдачноеX, double УдачноеY)
        {
            double Fangle = self.GetAngleTo(УдачноеX, УдачноеY);
            //Сообщить("Угол " + Fangle.ToString());
            ИдтиТуда(move, Fangle);
        }

        private void ИдтиТуда(Model.Move move, double Fangle)
        {

            if (Math.Abs(Fangle) > Math.PI * 0.9D)
            {
                
                move.Turn = (!ШайбаУМеня(s_elf, w_orld)) ? -Fangle : Fangle;
                move.SpeedUp = -1.0D;
            }
            else
            {
                move.SpeedUp = 1.0D;
                move.Turn = Fangle;
            }
            КорректироватьСкоростьДвижения(move);
        }

        private void КорректироватьСкоростьДвижения(Model.Move move)
        {
           
        }

        private void ИдтиКЦели(Hockeyist self, Model.Move move, Unit unit)
        {
            ИдтиКЦели(self, move, unit.X, unit.Y);
        }

        private bool ШайбаУМеня(Hockeyist self, World world)
        {
            return world.Puck.OwnerHockeyistId == self.Id;
        }

        private bool ШайбаУМоейКоманды(Hockeyist self, World world)
        {
            return world.Puck.OwnerPlayerId == self.PlayerId;
        }

        private void УдарПоВоротам(Hockeyist self, World world, Move move)
        {


            double angleToNet = self.GetAngleTo(netX, netY);
            move.Turn = angleToNet;
            if (self.State == HockeyistState.Swinging)
            {
                move.Action = ActionType.Strike;
                Сообщить("Атака");
                return;
            }
            if (Math.Abs(angleToNet) < STRIKE_ANGLE)
            {
                move.Action = ActionType.Swing;
                if (ВрагиБлизко(self, world, g_game.StickLength))
                {
                    Сообщить("Враг близко, бью напролом");
                    move.Action = ActionType.Strike; }
            }

        }


        
        private bool ВрагиБлизко(Hockeyist self, World world,double r)
        {
            return ОниБлизко(self, world, r);
        }

        private static bool ОниБлизко(Hockeyist self, World world, double r)
        {
            var Ищем = from Hockeyist игрок in world.Hockeyists
                       where
                           !игрок.IsTeammate
                           && игрок.GetDistanceTo(self) <= (r)
                       select игрок;
            bool Вражины = Ищем.FirstOrDefault() != null;
             
            return Вражины;
        }

 

        private void ПолучитьТочкуУдараПоВоротам(Hockeyist self, World world, out double x, out double y)
        {
            Player opponentPlayer = world.GetOpponentPlayer();
            var Вратарь = (from Hockeyist игрок in world.Hockeyists where !игрок.IsTeammate && игрок.Type == HockeyistType.Goalie select игрок).FirstOrDefault();
            x = opponentPlayer.NetFront;
            double СерединаВорот = (0.5D * (opponentPlayer.NetBottom + opponentPlayer.NetTop));
            if (Вратарь != null)
            {
                y = (Вратарь.Y < СерединаВорот) ? opponentPlayer.NetBottom  : opponentPlayer.NetTop ;
            }
            else
            {
                y = (0.5D * (opponentPlayer.NetBottom + opponentPlayer.NetTop));
            }
        }

        private void ДогнатьШайбу(Hockeyist self, World world, Move move)
        {
            double РазностьУглов = Math.Abs(world.Puck.Angle-world.Puck.GetAngleTo(self))/Math.PI;
            double КоэфициентОпереженияШайбы = 0.03D * РазностьУглов;
            double КоэфициентОпережения = self.GetDistanceTo(world.Puck) * world.Puck.Mass * КоэфициентОпереженияШайбы;
            double x = world.Puck.SpeedX * КоэфициентОпережения + world.Puck.X;
            double y = world.Puck.SpeedY * КоэфициентОпережения + world.Puck.Y;
            if (x < 0.0D) x = 0.0D;
            if (y < 0.0D) y = 0.0D;
            if (x > world.Width) x = world.Width;
            if (y > world.Height) y = world.Height;
            //double Fangle = self.GetAngleTo(x, y);
            //Сообщить("Угол " + Fangle.ToString());
            ИдтиКЦели(self, move, x, y);
            move.Action = ActionType.TakePuck;
            
        }



    }
}

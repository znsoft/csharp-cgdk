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
        private double УворотX;
        public static Game g_game;
        public static World Мир;
        public static Hockeyist s_elf;
        public static Hockeyist МойВратарь;
        public static Hockeyist Защитнег;
        //Move m_ove;
        public static Player Оппонент;
        public static Puck Шайба;
        public static double СерединаВорот;



        public void Move(Hockeyist self, World world, Game game, Move move)
        {
            ИнициализироватьПеременныеСеанса(self, world, game, move);
            if (ШайбаУМоейКоманды())
            {

                if (ШайбаУМеня())
                {

                    if (НахожусьВУдачномМесте(self))
                    {

                        if (УдачныйМомент())
                        {

                            УдарПоВоротам(move);
                            //Сообщить("атака");
                        }
                        else
                        {
                            ПопробоватьПасс(self, world, move);

                        }
                    }
                    else
                    {
                        if (ЯБлижеДругихК_СвоимВоротам(s_elf) && ВрагиБлизко(s_elf, g_game.StickLength * 4.0D + s_elf.Radius* 2.0D + Мир.Puck.Radius * 2.0D))
                        {
                            if (ДатьПасс(s_elf, Мир, move))
                                return;
                        }
                        ПопробоватьПасс(self, world, move);

                    }
                }
                else
                {
                    ВыбратьСтратегиюЗащиты(move);
                }

            }
            else
            {
                ВыбратьСтратегиюЗащиты(move);
            }

        }




        void ВыбратьСтратегиюЗащиты(Move move)
        {

            bool БлижеКВоротам = ЯБлижеДругихК_СвоимВоротам(s_elf);
            if (ЯБлижеДругихКШайбе(null)
                && (!БлижеКВоротам
               )
                )
            {

                ДогнатьШайбу(move);
            }
            else
            {
                //if(МоихИгроковНаПоле()==2 && Защитнег!=null && ! ШайбаНаМоейСторонеПоля() ){БитьПротивника(move,Защитнег);return;}

                if (БлижеКВоротам)
                {
                    БежатьЗащищатьВорота(move);

                }
                else
                {
                    if (ШайбаУМоейКоманды()) { БежатьВУдачнуюПозицию(move); return; }
                    if (Защитнег != null) { БитьПротивника(move, Защитнег); return; }
                    var НашихБьет = НашихБьют();
                    if (НашихБьет != null) { БитьПротивника(move, НашихБьет); return; }
                    БежатьВУдачнуюПозицию(move);
       

                }
            }
        }





        #region Инициализация
        /// <summary>
        /// Инициализироватьs the переменные сеанса.
        /// </summary>
        /// <param name="self">Self.</param>
        /// <param name="world">World.</param>
        /// <param name="game">Game.</param>
        /// <param name="move">Move.</param>
        void ИнициализироватьПеременныеСеанса(Hockeyist self, World world, Game game, Move move)
        {
            s_elf = self;
            g_game = game;
            Мир = world;
            //m_ove = move;
            Шайба = world.Puck;
            Оппонент = Мир.GetOpponentPlayer();
            ПолучитьРасположениеМоихВорот();
            ПолучитьУдачноеРасположениеДляУдараПоВоротамПротивника();
            Защитнег = СтратегияПротивника_ЗащищатьВорота_НайтиЭтогоЗащитника();

        }


        private void ПолучитьРасположениеМоихВорот()
        {
            Player my = Мир.GetMyPlayer();
            double x = my.NetFront;
            double Netshift = 1.5D * s_elf.Radius;
            Netshift = ((ЦентрПоляХ > my.NetFront) ? Netshift : -Netshift);
            ЦентрПоляХ = Мир.Width * 0.5D;
            ЦентрПоляY = 0.5D * (my.NetTop + my.NetBottom);

            МойВратарь = (from Hockeyist игрок in Мир.Hockeyists where игрок.IsTeammate && игрок.Type == HockeyistType.Goalie select игрок).FirstOrDefault();
            if (МойВратарь == null)
            {
                //МойВратарь = s_elf;
            }
            else { x = МойВратарь.X; }
            УворотX = x + Netshift;
        }
        private void ПолучитьУдачноеРасположениеДляУдараПоВоротамПротивника()
        {
            УдачноеY = ЦентрПоляY * 0.5D;
            if ((s_elf.Y + s_elf.SpeedY * s_elf.Mass * 0.01D) > ЦентрПоляY)
                УдачноеY = (ЦентрПоляY + Мир.Height) * 0.5D;
            УдачныйРадиусY = (Мир.Width / 6.0D) - 50.0D;
            УдачноеX = (ЦентрПоляХ + Оппонент.NetFront) * 0.5D;
            УдачноеX = (ЦентрПоляХ + УдачноеX) * 0.5D;
            УдачныйРадиусX = Math.Abs(Мир.Width / 12.0D);
            //УдачноеX = (УдачноеX + ЦентрПоляХ) * 0.5D;
        }

        private void ПолучитьТочкуУдараПоВоротам(out double x, out double y)
        {

            var Вратарь = (from Hockeyist игрок in Мир.Hockeyists where !игрок.IsTeammate && игрок.Type == HockeyistType.Goalie select игрок).FirstOrDefault();
            x = 0.5D * (Оппонент.NetFront + Оппонент.NetBack);
            double СерединаВорот = (0.5D * (Оппонент.NetBottom + Оппонент.NetTop));
            if (Вратарь != null)
            {
                y = (Вратарь.Y < СерединаВорот) ? Оппонент.NetBottom : Оппонент.NetTop;
            }
            else
            {
                y = (0.5D * (Оппонент.NetBottom + Оппонент.NetTop));
            }
        }
        #endregion
        #region Вопросы
        //---------------- Вопросы -------------------
        private bool ШайбаУМеня()
        {
            return Мир.Puck.OwnerHockeyistId == s_elf.Id;
        }

        private bool ШайбаУМоейКоманды()
        {
            return Мир.Puck.OwnerPlayerId == s_elf.PlayerId;
        }



        private bool НахожусьВообщеДалеко(Hockeyist self)
        {
            return Math.Abs(s_elf.X - netX) > Math.Abs(ЦентрПоляХ - netX);
        }

        private bool НахожусьВУдачномXМесте(Hockeyist self)
        {
            return (self.X < УдачноеX + УдачныйРадиусX)
                && (self.X > УдачноеX - УдачныйРадиусX);

        }

        private bool НахожусьВУдачномМесте(Hockeyist self)
        {
            return (self.X < УдачноеX + УдачныйРадиусX)
                && (self.X > УдачноеX - УдачныйРадиусX)
                    && (self.Y < УдачноеY + УдачныйРадиусY)
                    && (self.Y > УдачноеY - УдачныйРадиусY);

        }

        private bool ШайбаНаМоейСторонеПоля()
        {

            return Math.Sign(Шайба.X - ЦентрПоляХ) == Math.Sign(ЦентрПоляХ - УдачноеX);

        }


        private bool УдачныйМомент()
        {
            ПолучитьТочкуУдараПоВоротам(out netX, out netY);
            return (КтоНаЛинииОгня(netX, netY) == null);
        }

        private Hockeyist КтоНаЛинииОгня(double x, double y)
        {

            var Ищем = from Hockeyist игрок in Мир.Hockeyists
                       where
                       !игрок.IsTeammate && (Math.Abs(игрок.GetAngleTo(Шайба)) < STRIKE_ANGLE * 2.0D || игрок.Type == HockeyistType.Goalie)
                       && НаЛинии(s_elf, x, y, игрок, 0.4D)
                       select игрок;
            Hockeyist ИгрокНаПути = Ищем.FirstOrDefault();
            return ИгрокНаПути;

        }

        private bool НаЛинии(Hockeyist self, double x, double y, Hockeyist игрок, double p)
        {
            double ty = self.Y + ((игрок.X - x) * (y - self.Y)) / (x - self.X);
            return Math.Abs(ty - игрок.Y) < (игрок.Radius + Шайба.Radius ) * p ;
        }

        bool ЯБлижеДругихК_СвоимВоротам(Hockeyist self)
        {
            double МоеРасстояниеДоВорот = self.GetDistanceTo(УворотX, ЦентрПоляY);
            var ИщемСвоего = from Hockeyist игрок in Мир.Hockeyists
                             where
                                 игрок.Id != self.Id && игрок.IsTeammate
                                 && игрок.Type != HockeyistType.Goalie
                                 && игрок.State != HockeyistState.Resting
                                 && игрок.GetDistanceTo(УворотX, ЦентрПоляY) < МоеРасстояниеДоВорот
                             select игрок;
            return ИщемСвоего.Count() == 0;
        }

        private bool ЯБлижеДругихКШайбе(Hockeyist Защитник)
        {
            double МоеРасстояние = s_elf.GetDistanceTo(Мир.Puck);
            double МоеРасстояниеДоВорот = s_elf.GetDistanceTo(УворотX, ЦентрПоляY);
            double МоеРасстояниеДоЗащитника = (Защитник == null) ? 0 : s_elf.GetDistanceTo(Защитник);
            var ИщемСвоего = from Hockeyist игрок in Мир.Hockeyists
                             where
                                 игрок.Id != s_elf.Id && игрок.IsTeammate
                                 && игрок.Type != HockeyistType.Goalie
                                 //&& НахожусьВообщеДалеко (игрок)
                                 //&& игрок.State != HockeyistState.KnockedDown 
                                 && игрок.State != HockeyistState.Resting
                                 && игрок.GetDistanceTo(Мир.Puck) < МоеРасстояние
                                 && (игрок.GetDistanceTo(УворотX, ЦентрПоляY) > МоеРасстояниеДоВорот
                                 //|| ((Защитник == null) ? 0 : игрок.GetDistanceTo(Защитник)) > МоеРасстояниеДоЗащитника
                                 )
                             select игрок;
            return ИщемСвоего.Count() == 0;

        }

        private bool ЯБлижеДругихК(Unit u)
        {
            double МоеРасстояние = s_elf.GetDistanceTo(u);
            var ИщемСвоего = from Hockeyist игрок in Мир.Hockeyists
                             where
                                 игрок.Id != s_elf.Id && игрок.IsTeammate
                                 && игрок.Type != HockeyistType.Goalie
                                 && игрок.State != HockeyistState.Resting
                                 && игрок.GetDistanceTo(u) < МоеРасстояние
                             select игрок;
            return ИщемСвоего.Count() == 0;

        }

        private bool ВрагиБлизко(Hockeyist self, double r)
        {
            return ОниБлизко(self, r);
        }

        private bool ОниБлизко(Hockeyist self, double r)
        {
            var Ищем = from Hockeyist игрок in Мир.Hockeyists
                       where
                           !игрок.IsTeammate
                           && игрок.GetDistanceTo(self) < (r) 
                           && (Math.Abs(игрок.GetAngleTo(self)) < STRIKE_ANGLE * 15.0D)
                       select игрок;
            bool Вражины = Ищем.FirstOrDefault() != null;
            return Вражины;
        }



        private bool ВрагиПередоМной(Hockeyist self, double r)
        {
            return ВрагиПередоМной(self, r, 10.0D);
        }


        /// <summary>
        /// узнаем что враги перед нами
        /// </summary>
        /// <param name="self">игрок</param>
        /// <param name="r">в пределах радиуса</param>
        /// <param name="градусы">угол обзора перед собой * 2 , из за симметрии</param>
        /// <returns></returns>
        private bool ВрагиПередоМной(Hockeyist self, double r, double градусы)
        {
            var Ищем = from Hockeyist игрок in Мир.Hockeyists
                       where игрок.Type != HockeyistType.Goalie &&
                           игрок.State != HockeyistState.KnockedDown &&
                           игрок.State != HockeyistState.Resting &&
                           !игрок.IsTeammate
                           && игрок.GetDistanceTo(self) <= (r) &&
                           Math.Abs(self.GetAngleTo(игрок)) < (градусы * Math.PI / 180.0D)
                       select игрок;
            bool Вражины = Ищем.FirstOrDefault() != null;
            return Вражины;
        }

        bool ШайбаЛетитНаМеня(double Опережение)
        {
            double x = Шайба.X + Шайба.SpeedX * Опережение;
            double y = Шайба.Y + Шайба.SpeedY * Опережение;

            return s_elf.GetDistanceTo(x, y) < g_game.StickLength;
        }

        bool ШайбаСвободна(){
            return Шайба.OwnerHockeyistId == null;
        }

        Puck ШайбаЛетитКоВворота() {

            double Расстояние = Шайба.GetDistanceTo(УворотX, ЦентрПоляY);
            double x = Шайба.X + Шайба.SpeedX * Расстояние;
            double y = Шайба.Y + Шайба.SpeedY * Расстояние;
            return new Puck(999999, Шайба.Mass, Шайба.Radius, x, y, Шайба.SpeedX, Шайба.SpeedY, Шайба.OwnerHockeyistId, Шайба.OwnerPlayerId);
        }

        private Hockeyist НашихБьют()
        {

            Hockeyist Мой = (from Hockeyist Owner in Мир.Hockeyists
                             where
                                 Owner.Id == Шайба.OwnerHockeyistId
                             select Owner).FirstOrDefault();
            if (Мой == null)
                return null;

            Hockeyist ПристаетКМоему = (from Hockeyist Враг in Мир.Hockeyists where Враг.GetDistanceTo(Мой) < g_game.StickLength select Враг).FirstOrDefault();
            return ПристаетКМоему;


        }

        private Hockeyist ИгрокГотовПринять()
        {

            var ИщемСвоего = from Hockeyist игрок in Мир.Hockeyists
                             where
                                 игрок.Id != s_elf.Id && игрок.IsTeammate
                                 && игрок.Type != HockeyistType.Goalie
                                 && игрок.State == HockeyistState.Active
                                 && (НахожусьВУдачномXМесте(игрок) || !ЯБлижеДругихК_СвоимВоротам(игрок))
                                 && !ВрагиБлизко(игрок, игрок.Radius + g_game.StickLength)
                                 && КтоНаЛинииОгня(игрок.X, игрок.Y) == null 
                             select игрок;
            return ИщемСвоего.FirstOrDefault();
        }


        Hockeyist СтратегияПротивника_ЗащищатьВорота_НайтиЭтогоЗащитника()
        {

            return (from Hockeyist Враг in Мир.Hockeyists
                    where Враг.Type != HockeyistType.Goalie
                        && !Враг.IsTeammate
                        && Враг.GetDistanceTo(Оппонент.NetFront, ЦентрПоляY) < (8.0D * Враг.Radius)
                        && Враг.SpeedX < 0.01D
                        && Враг.SpeedY < 0.01D
                    //					&& Math.Abs(Враг.GetAngleTo(Мир.Puck)) < 0.2D
                    select Враг).FirstOrDefault();

        }


        int МоихИгроковНаПоле()
        {
            return (from Hockeyist Игрок in Мир.Hockeyists
                    where Игрок.Type != HockeyistType.Goalie
                    && Игрок.IsTeammate
                    select Игрок).Count();
        }


        private void ЗажалиВУглу(ref double Fangle)
        {
            double Дальность = 3.0D;
            if ((s_elf.X + s_elf.Radius + s_elf.SpeedX * Дальность) >= Мир.Width)
                Fangle -= 0.5D;
            if ((s_elf.X - s_elf.Radius + s_elf.SpeedX * Дальность) <= 0.0D)
                Fangle += 0.5D;
            if ((s_elf.Y + s_elf.Radius + s_elf.SpeedY * Дальность) >= Мир.Height)
                Fangle -= 0.5D;
            if ((s_elf.Y - s_elf.Radius + s_elf.SpeedY * Дальность) <= 0.0D)
                Fangle += 0.5D;
        }

        #endregion
        //--------------------------- действия -------------------
        #region Движения
        // ------Движения ------------

        private void БежатьВУдачнуюПозицию(Move move)
        {
            if (НахожусьВУдачномМесте(s_elf))
            {
                double angleToNet = s_elf.GetAngleTo(netX, netY);
                move.Turn = angleToNet;
                move.SpeedUp = -0.5D;
                if (ВрагиБлизко(s_elf, s_elf.Radius))
                    move.SpeedUp = -1.0D;
            }
            else
            {
                ИдтиКЦели(move, УдачноеX, УдачноеY);

            }
        }




        private bool УбежалОтВрагов(Move move)
        {
            double r = 2.0D * Шайба.Radius + g_game.StickLength;

            var ВрагиКругом = from Hockeyist игрок in Мир.Hockeyists
                              where !игрок.IsTeammate
                                  && s_elf.GetDistanceTo(игрок) < (r)
                                  && Math.Abs(s_elf.GetAngleTo(игрок)) < 0.3D
                              select игрок;
            if (ВрагиКругом.Count() == 0)
                return true;
            double x = 0.0D, y = 0.0D;
            foreach (var игрок in ВрагиКругом)
            {
                x += (игрок.X + игрок.SpeedX);
                x *= 0.5D;
                y += (игрок.SpeedY + игрок.Y);
                y *= 0.5D;
            }
            double Fangle = -s_elf.GetAngleTo(x, y);
            ИдтиТуда(move, Fangle);
            return false;
        }


        private void ИдтиКЦели(Move move, double УдачноеX, double УдачноеY)
        {
            double Fangle = s_elf.GetAngleTo(УдачноеX, УдачноеY);
            ИдтиТуда(move, Fangle);
        }


        /// <summary>
        ///  процедура движения игрока в точку
        /// </summary>
        /// <param name="move"></param>
        /// <param name="УдачноеX"></param>
        /// <param name="УдачноеY"></param>
        /// <param name="back">допуск движения задом к точке ( 1.0 = нельзя)</param>
        /// <param name="speed"></param>
        private void ИдтиКЦели(Move move, double УдачноеX, double УдачноеY, double back, double speed)
        {
            double Fangle = s_elf.GetAngleTo(УдачноеX, УдачноеY);
            //Сообщить("Угол " + Fangle.ToString());
            ИдтиТуда(move, Fangle, back, speed);
        }
        private void ИдтиТуда(Move move, double Fangle)
        {
            ИдтиТуда(move, Fangle, 0.99D, 1.0D);
        }

        /// <summary>
        /// процедура движения игрока в точку
        /// </summary>
        /// <param name="move"> клас движения </param>
        /// <param name="Fangle"> угол поворота  </param>
        /// <param name="back">допуск движения задом к точке ( 1.0 = нельзя)</param>
        /// <param name="speed">скорость</param>
        private void ИдтиТуда(Model.Move move, double Fangle, double back, double speed)
        {

            if (Math.Abs(Fangle) > Math.PI * back)
            {

                //				move.Turn = (!ШайбаУМеня(s_elf, w_orld)) ? -Fangle : Fangle;
                move.Turn = -Fangle;
                move.SpeedUp = -speed;
            }
            else
            {
                move.SpeedUp = speed;
                move.Turn = Fangle;
            }
            КорректироватьСкоростьДвижения(move);
        }

        private void КорректироватьСкоростьДвижения(Model.Move move)
        {

        }

        private void ИдтиКЦели(Move move, Unit unit)
        {
            ИдтиКЦели(move, unit.X, unit.Y);
        }




        private void ДогнатьШайбу(Move move)
        {
            double dist = s_elf.GetDistanceTo(Шайба);
            double РазностьУглов = Math.Abs(Шайба.GetAngleTo(s_elf));
            double КоэфициентОпереженияШайбы = РазностьУглов * 12.9D;
            double КоэфициентОпережения = dist / Мир.Height * Шайба.Mass * КоэфициентОпереженияШайбы;
            if (dist < (Шайба.Radius + s_elf.Radius))
                КоэфициентОпережения = 1.0D;
            double x = Шайба.SpeedX * КоэфициентОпережения + Шайба.X;
            double y = Шайба.SpeedY * КоэфициентОпережения + Шайба.Y;
            if (x < 0.0D)
                x = 0.0D;
            if (y < 0.0D)
                y = 0.0D;
            if (x > Мир.Width)
                x = Мир.Width;
            if (y > Мир.Height)
                y = Мир.Height;
            ИдтиКЦели(move, x, y);
            move.Action = ActionType.TakePuck;
            if (ВрагиПередоМной(s_elf, g_game.StickLength)) move.Action = ActionType.Strike;


        }


        private void БежатьЗащищатьВорота(Move move)
        {
            move.Action = ActionType.TakePuck;
            double Y = ЦентрПоляY;
           
            if (МойВратарь != null)
            {
                Y = ЦентрПоляY - (МойВратарь.Y - ЦентрПоляY) * 0.6D;
            }
            double dist = s_elf.GetDistanceTo(УворотX, Y);

            if (ВрагиБлизко(s_elf, g_game.StickLength * 0.5D) || (ШайбаЛетитНаМеня(0.0D) && Math.Abs(s_elf.GetAngleTo(Шайба)) > STRIKE_ANGLE * 2.0D))
            {
                move.Action = ActionType.Strike;
                move.SpeedUp = 0.0D;
                //Сообщить("отбиваюсь");
            }
            else
            {

                //double Допуск = 
                ИдтиКЦели(move, УворотX, Y, 0.8D, 0.5D * dist / s_elf.Radius);
                if (dist < 1.3D * s_elf.Radius && Math.Abs(Y - s_elf.Y) < s_elf.Radius)
                {
                    move.SpeedUp = 0.0D;
                    move.Turn = s_elf.GetAngleTo(Шайба);
                }
                else {
                    if (ПротивникСшайбойНаМоемПоле()) {
                        move.SpeedUp = 0.0D;
                        move.Turn = s_elf.GetAngleTo(Шайба);
                    }
                
                }
            }
            if (ШайбаСвободна()) {
                Puck _Шайба = ШайбаЛетитКоВворота();
                Player my = Мир.GetMyPlayer();
                double yr = Math.Abs(my.NetBottom - my.NetTop)*0.5D;
                if(_Шайба.GetDistanceTo(УворотX, ЦентрПоляY) < yr ){
                    move.SpeedUp = 0.0D;
                    move.Turn = s_elf.GetAngleTo(Шайба);
                    Сообщить("Ой сейчас мне забьют");
                }
            
            }

        }

        private bool ПротивникСшайбойНаМоемПоле()
        {
            return ШайбаНаМоейСторонеПоля() && !ШайбаУМоейКоманды();
        }

        void БитьПротивника(Move move, Hockeyist ПристаетКМоему)
        {
            if (ПристаетКМоему == null)
                return;
            Сообщить("побежал бить защитника");
            ИдтиКЦели(move, ПристаетКМоему.X, ПристаетКМоему.Y);
            if (ВрагиПередоМной(s_elf, g_game.StickLength)) move.Action = ActionType.Strike;
            return;
        }



        private void ЗанятьУдачнуюПозициюНаПоле(Move move)
        {
            move.Action = ActionType.TakePuck;
            bool МояШайба = ШайбаУМеня();
            if (!МояШайба)
            {
                if (ВрагиБлизко(s_elf, g_game.StickLength * 0.5D))
                {
                    move.Action = ActionType.Strike;
                }
                if (ШайбаУМоейКоманды())
                    if (ШайбаНаМоейСторонеПоля())
                    {
                        ВыбратьСтратегиюЗащиты(move);
                        return;
                    }

            }


            if (НахожусьВУдачномМесте(s_elf))
            {
                double angleToNet = s_elf.GetAngleTo(netX, netY);
                move.Turn = angleToNet;
                move.SpeedUp = -0.5D;
                if (ВрагиБлизко(s_elf, s_elf.Radius))
                    move.SpeedUp = -1.0D;
            }
            else
            {
                if (МояШайба)
                {
                    if (УбежалОтВрагов(move))
                        ИдтиКЦели(move, УдачноеX, УдачноеY);
                }
                else
                {
                    ИдтиКЦели(move, УдачноеX, УдачноеY);
                }
            }
        }





        #endregion

        #region Удары
        //----------------удары --------------
        private void УдарПоВоротам(Move move)
        {
            double angleToNet = s_elf.GetAngleTo(netX, netY);
            move.Turn = angleToNet;
            if (s_elf.State == HockeyistState.Swinging)
            {
                move.Action = ActionType.Strike;
                //Сообщить("Атака");
                return;
            }
            if (Math.Abs(angleToNet) < STRIKE_ANGLE)
            {
                move.Action = ActionType.Swing;
                if (ВрагиБлизко(s_elf, g_game.StickLength))
                {
                    //Сообщить("Враг близко, бью напролом");
                    move.Action = ActionType.Strike;
                }
            }

        }


        private bool ДатьПасс(Hockeyist self, World world, Model.Move move)
        {

            Hockeyist ПринимающийИгрок = ИгрокГотовПринять();
            if (ПринимающийИгрок == null)
                return false;
            move.PassPower = world.Height / self.GetDistanceTo(ПринимающийИгрок);
            if (move.PassPower < 0.2D)
                move.PassPower = 1.0D;
            double Угол = self.GetAngleTo(ПринимающийИгрок);
            move.Turn = Угол;
            move.PassAngle = Угол;

            if (Math.Abs(Угол) < STRIKE_ANGLE * 2.0D)
            {
                move.Action = ActionType.Pass;
                //Сообщить("Пасс " + ПринимающийИгрок.Id.ToString());
            }
            return true;
        }



        private bool ПопробоватьПасс(Hockeyist self, World world, Move move)
        {
            if (ВрагиБлизко(s_elf, g_game.StickLength + self.Radius))
                if (ДатьПасс(self, world, move))
                    return true;
            ЗанятьУдачнуюПозициюНаПоле(move);
            if (ВрагиПередоМной(s_elf, g_game.StickLength + s_elf.Radius, 15.0D)) move.Action = ActionType.Strike;
            return false;
        }




        #endregion





        private void Сообщить(string Строка)
        {
            //if ((w_orld.Tick & 1) == 1)
            System.Console.Out.WriteLine(s_elf.Id.ToString() + " " + Строка + "  " + Мир.Tick.ToString());
        }
    }
}


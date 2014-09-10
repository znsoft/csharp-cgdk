using System.Linq;
using System;
using Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk.Model;


namespace Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk
{
    public sealed class MyStrategy : IStrategy
    {
        private static double STRIKE_ANGLE = 1.0D * Math.PI / 180.0D;
        private double netX;
        private double netY;
        private double ЦентрПоляХ;
        private double УдачноеX;
        private double УдачноеY;
        private double УдачныйРадиус;


        public void Move(Hockeyist self, World world, Game game, Move move)
        {

            if (ШайбаУМоейКоманды(self, world))
            {
                ПолучитьУдачноеРасположение(world);

                if (ШайбаУМеня(self, world))
                {
                    
                    if (НахожусьВУдачномМесте(self))
                    {

                        if (УдачныйМомент(self, world))
                        {
                            Сообщить(self.Id.ToString() + " Атака");
                            УдарПоВоротам(self, world, move);
                        }
                        else
                        {
                            if (!ДатьПасс(self, world, move)) ЗанятьУдачнуюПозициюНаПоле(self, world, move);
                        }
                    }
                    else
                    {
                        
                        ЗанятьУдачнуюПозициюНаПоле(self, world, move);
                        if (НахожусьВообщеДалеко(self)) ДатьПасс(self, world, move);

                    }
                }
                else
                {
                    ЗанятьУдачнуюПозициюНаПоле(self, world, move);
                    Сообщить(self.Id.ToString() + " Переход");
                }

            }
            else
            {
                ДогнатьШайбу(self, world, move);
                Сообщить(self.Id.ToString());
            }

        }

        private bool НахожусьВообщеДалеко(Hockeyist self)
        {
            return Math.Abs(self.X - netX) > Math.Abs(ЦентрПоляХ - netX);
        }

        private bool НахожусьВУдачномМесте(Hockeyist self)
        {
            return (self.X < УдачноеX + УдачныйРадиус) && (self.X > УдачноеX - УдачныйРадиус) && (self.Y < УдачноеY + УдачныйРадиус) && (self.Y > УдачноеY - УдачныйРадиус);

        }

        private void ПолучитьУдачноеРасположение(World world)
        {
            Player opponentPlayer = world.GetOpponentPlayer();
            ЦентрПоляХ = world.Width * 0.5D;
            double ЦентрПоляY = world.Height * 0.5D;
            УдачноеY = ЦентрПоляY;
            УдачныйРадиус = Math.Abs(0.5D * (opponentPlayer.NetBottom - opponentPlayer.NetTop));
            УдачноеX = (ЦентрПоляХ + opponentPlayer.NetFront) * 0.5D;
        }

        private void Сообщить(string Строка)
        {

            System.Console.WriteLine(Строка);
        }

        private bool ДатьПасс(Hockeyist self, World world, Model.Move move)
        {
            move.PassPower = 0.8D;
            Hockeyist ПринимающийИгрок = ИгрокГотовПринять(self, world, move);
            if (ПринимающийИгрок == null) return false;
            move.PassAngle = self.GetAngleTo(ПринимающийИгрок);
            move.Action = ActionType.Pass;
            if (ПринимающийИгрок != null) Сообщить(self.Id.ToString() + " Пассую " + ПринимающийИгрок.Id.ToString());
            return true;
        }

        private Hockeyist ИгрокГотовПринять(Hockeyist self, World world, Model.Move move)
        {

            var ИщемСвоего = from Hockeyist игрок in world.Hockeyists where игрок.Id != self.Id && игрок.IsTeammate && игрок.Type != HockeyistType.Defenceman && КтоНаЛинииОгня(self, world, игрок.X, игрок.Y) == null select игрок;
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
            if (ИгрокНаПути != null) Сообщить("y " + self.Id.ToString() + " Игрок на пути " + ИгрокНаПути.Id.ToString());
            return ИгрокНаПути;

        }

        private bool НаЛинии(Hockeyist self, double x, double y, Hockeyist игрок, double p)
        {
            double ty = self.Y + ((игрок.X - x) * (y - self.Y)) / (x - self.X);
            return Math.Abs(ty - игрок.Y) < игрок.Radius + p;
        }



        private void ЗанятьУдачнуюПозициюНаПоле(Hockeyist self, World world, Model.Move move)
        {
            move.Action = ActionType.TakePuck;
            if (НахожусьВУдачномМесте(self))
            {
                double angleToNet = self.GetAngleTo(netX, netY);
                move.Turn = angleToNet;
                move.SpeedUp = 0.0D;
                if (!ШайбаУМеня(self, world)) move.SpeedUp = -0.5D;
                Сообщить("y " + self.Id.ToString() + " Подготовка");

            }
            else
            {
                ИдтиКЦели(self, move, УдачноеX, УдачноеY);
                Сообщить("y " + self.Id.ToString() + " Переход в центр");
            }
        }

        private void ИдтиКЦели(Hockeyist self, Model.Move move, double УдачноеX, double УдачноеY)
        {
            double Fangle = self.GetAngleTo(УдачноеX, УдачноеY);
            if (Math.Abs(Fangle) > Math.PI)
            {
                move.SpeedUp = -1.0D;
                move.Turn = -Fangle;
            }
            else
            {
                move.SpeedUp = 1.0D;
                move.Turn = Fangle;
            }
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
                return;
            }
            if (Math.Abs(angleToNet) < STRIKE_ANGLE)
            {
                move.Action = ActionType.Swing;
                if (ВрагиБлизко(self, world)) move.Action = ActionType.Strike;
            }

        }

        private bool ВрагиБлизко(Hockeyist self, World world)
        {
            var Ищем = from Hockeyist игрок in world.Hockeyists where !игрок.IsTeammate && ((Math.Abs(self.X - игрок.X) - (self.Radius + игрок.Radius)) < (self.Radius + игрок.Radius)) && ((Math.Abs(self.Y - игрок.Y) - (self.Radius + игрок.Radius)) < (self.Radius + игрок.Radius)) select игрок;
            return Ищем.FirstOrDefault() != null;
        }

        private void ПолучитьТочкуУдараПоВоротам(Hockeyist self, World world, out double x, out double y)
        {
            Player opponentPlayer = world.GetOpponentPlayer();
            double ближВерх = self.GetDistanceTo(opponentPlayer.NetFront, opponentPlayer.NetTop);
            double ближНиз = self.GetDistanceTo(opponentPlayer.NetFront, opponentPlayer.NetBottom);
            double mix = (opponentPlayer.NetBottom - opponentPlayer.NetTop) / 4.0D;

            if (ближВерх > ближНиз) mix = -mix;
            x = opponentPlayer.NetFront;
            y = (0.5D * (opponentPlayer.NetBottom + opponentPlayer.NetTop)) + mix;
        }

        private void ДогнатьШайбу(Hockeyist self, World world, Move move)
        {
            ИдтиКЦели(self, move, world.Puck);
            move.Action = ActionType.TakePuck;
        }

        

    }
}

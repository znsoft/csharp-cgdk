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

        public void Move(Hockeyist self, World world, Game game, Move move)
        {

            if (ШайбаВМоейКоманде(self, world))
            {

                if (ШайбаУМеня(self, world))
                {
                    if (УдачныйМомент(self, world))
                    {
						Сообщить(self.Id.ToString()+" Атака");
                        УдарПоВоротам(self, world, move);
                    }
                    else
                    {
                        ДатьПасс(self, world, move);
                    }
                }
                else
                {

                    ПерейтиНаДругуюСторонуПоля(self, world, move);
					Сообщить(self.Id.ToString()+" Переход");
                }

            }
            else
            {
                ДогнатьШайбу(self, world, move);
				Сообщить(self.Id.ToString());
            }

        }

		private void Сообщить(string Строка){
		
				System.Console.WriteLine(Строка);
		}

        private void ДатьПасс(Hockeyist self, World world, Model.Move move)
        {
            move.PassPower = 1.0D;
			Hockeyist ПринимающийИгрок = ИгрокГотовПринять(self, world, move);
				move.PassAngle = self.GetAngleTo(ПринимающийИгрок);
            move.Action = ActionType.Pass;
			if(ПринимающийИгрок != null)Сообщить(self.Id.ToString()+" Пассую "+ ПринимающийИгрок.Id.ToString());
        }

        private Hockeyist ИгрокГотовПринять(Hockeyist self, World world, Model.Move move)
        {

			var ИщемСвоего = from Hockeyist игрок in world.Hockeyists where игрок.Id != self.Id && игрок.IsTeammate && игрок.Type != HockeyistType.Defenceman  select игрок;
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
			if(ИгрокНаПути != null)Сообщить("y "+self.Id.ToString()+" Игрок на пути "+ИгрокНаПути.Id.ToString());
				return ИгрокНаПути;

        }

        private bool НаЛинии(Hockeyist self, double x, double y, Hockeyist игрок, double p)
        {
            double ty = self.Y + ((игрок.X - x) * (y - self.Y)) / (x - self.X);
            return Math.Abs(ty - игрок.Y) < игрок.Radius;
        }



        private void ПерейтиНаДругуюСторонуПоля(Hockeyist self, World world, Model.Move move)
        {
            Player opponentPlayer = world.GetOpponentPlayer();
            double netX = opponentPlayer.NetFront;
            double УШайба = world.Puck.Y;
            double netY = (0.5D * (opponentPlayer.NetBottom + opponentPlayer.NetTop));

            move.SpeedUp = 1.0D;
            move.Turn = (self.GetAngleTo(netX, (УШайба > netY) ? 0 : world.Height));
            move.Action = ActionType.TakePuck;
        }

        private bool ШайбаУМеня(Hockeyist self, World world)
        {
            return world.Puck.OwnerHockeyistId == self.Id;
        }

        private bool ШайбаВМоейКоманде(Hockeyist self, World world)
        {
            return world.Puck.OwnerPlayerId == self.PlayerId;
        }

        private void УдарПоВоротам(Hockeyist self, World world, Move move)
        {
            if (self.State == HockeyistState.Swinging)
            {
                move.Action = ActionType.Strike;
                return;
            }


            double angleToNet = self.GetAngleTo(netX, netY);
            move.Turn = angleToNet;
			move.Action = ActionType.Swing;


			move.PassPower = 1.0D;
			move.PassAngle = self.GetAngleTo(netX, netY);
			move.Action = ActionType.Pass;

            if (Math.Abs(angleToNet) < STRIKE_ANGLE)
            {
                //
                //move.Action = ActionType.Strike;

            }

        }

        private void ПолучитьТочкуУдараПоВоротам(Hockeyist self, World world, out double x, out double y)
        {
            Player opponentPlayer = world.GetOpponentPlayer();
            double ближВерх = self.GetDistanceTo(opponentPlayer.NetFront, opponentPlayer.NetTop);
            double ближНиз = self.GetDistanceTo(opponentPlayer.NetFront, opponentPlayer.NetBottom);
            double mix = 15.0D;
            if (ближВерх > ближНиз) mix = -15.0D;
            x = 0.5D * (opponentPlayer.NetBack + opponentPlayer.NetFront);
            y = (0.5D * (opponentPlayer.NetBottom + opponentPlayer.NetTop)) + mix;
        }

        private void ДогнатьШайбу(Hockeyist self, World world, Move move)
        {
            move.SpeedUp = 1.0D;
            move.Turn = (self.GetAngleTo(world.Puck));
            move.Action = ActionType.TakePuck;
        }




    }
}

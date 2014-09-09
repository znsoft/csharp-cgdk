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

            if (Ўайба¬ћоей оманде(self, world))
            {

                if (Ўайба”ћен€(self, world))
                {
                    if (”дачныйћомент(self, world))
                    {
                        ”дарѕо¬оротам(self, world, move);
                    }
                    else
                    {
                        ƒатьѕасс(self, world, move);
                    }
                }
                else
                {

                    ѕерейтиЌаƒругую—торонуѕол€(self, world, move);

                }



            }
            else
            {
                ЋовитьЎайбу(self, world, move);
            }

        }

        private void ƒатьѕасс(Hockeyist self, World world, Model.Move move)
        {
            move.PassPower = 1.0D;
            move.PassAngle = self.GetAngleTo(»грок√отовѕрин€ть(self, world, move));
            move.Action = ActionType.Pass;
        }

        private Hockeyist »грок√отовѕрин€ть(Hockeyist self, World world, Model.Move move)
        {

            var »щем—воего = from Hockeyist игрок in world.Hockeyists where игрок.Id != self.Id && игрок.IsTeammate select игрок;
            return »щем—воего.FirstOrDefault();
        }

        private bool ”дачныйћомент(Hockeyist self, World world)
        {
            ѕолучить“очку”дараѕо¬оротам(self, world, out netX, out netY);
            return ( тоЌаЋинииќгн€(self, world, netX, netY) == null);
        }

        private Hockeyist  тоЌаЋинииќгн€(Hockeyist self, World world, double x, double y)
        {

            var »щем = from Hockeyist игрок in world.Hockeyists where !игрок.IsTeammate && игрок.Id != self.Id && ЌаЋинии(self, x, y, игрок, world.Puck.Radius) select игрок;
            return »щем.FirstOrDefault();

        }

        private bool ЌаЋинии(Hockeyist self, double x, double y, Hockeyist игрок, double p)
        {
            double ty = self.Y + ((игрок.X - x) * (y - self.Y)) / (x - self.X);
            return Math.Abs(ty - игрок.Y) < игрок.Radius;
        }



        private void ѕерейтиЌаƒругую—торонуѕол€(Hockeyist self, World world, Model.Move move)
        {
            Player opponentPlayer = world.GetOpponentPlayer();
            double netX = opponentPlayer.NetFront;
            double ”Ўайба = world.Puck.Y;
            double netY = (0.5D * (opponentPlayer.NetBottom + opponentPlayer.NetTop));

            move.SpeedUp = 1.0D;
            move.Turn = (self.GetAngleTo(netX, (”Ўайба > netY) ? 0 : world.Height));
            move.Action = ActionType.TakePuck;
        }

        private bool Ўайба”ћен€(Hockeyist self, World world)
        {
            return world.Puck.OwnerHockeyistId == self.Id;
        }

        private bool Ўайба¬ћоей оманде(Hockeyist self, World world)
        {
            return world.Puck.OwnerPlayerId == self.PlayerId;
        }

        private void ”дарѕо¬оротам(Hockeyist self, World world, Move move)
        {
            if (self.State == HockeyistState.Swinging)
            {
                move.Action = ActionType.Strike;
                return;
            }


            double angleToNet = self.GetAngleTo(netX, netY);
            move.Turn = angleToNet;
            if (Math.Abs(angleToNet) < STRIKE_ANGLE)
            {
                //                move.Action = ActionType.Swing;
                move.Action = ActionType.Strike;
            }

        }

        private void ѕолучить“очку”дараѕо¬оротам(Hockeyist self, World world, out double x, out double y)
        {
            Player opponentPlayer = world.GetOpponentPlayer();
            double ближ¬ерх = self.GetDistanceTo(opponentPlayer.NetFront, opponentPlayer.NetTop);
            double ближЌиз = self.GetDistanceTo(opponentPlayer.NetFront, opponentPlayer.NetBottom);
            double mix = 5.0D;
            if (ближ¬ерх > ближЌиз) mix = -5.0D;
            x = 0.5D * (opponentPlayer.NetBack + opponentPlayer.NetFront);
            y = (0.5D * (opponentPlayer.NetBottom + opponentPlayer.NetTop)) + mix;
        }

        private void ЋовитьЎайбу(Hockeyist self, World world, Move move)
        {
            move.SpeedUp = 1.0D;
            move.Turn = (self.GetAngleTo(world.Puck));
            move.Action = ActionType.TakePuck;
        }




    }
}
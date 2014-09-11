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
					//if(враги на линии)БежатьКСвоимВоротам(self, world, move);
					ЗанятьУдачнуюПозициюНаПоле(self, world, move);
				}

			}
			else
			{
				ДогнатьШайбу(self, world, move);
			}

		}

		void БежатьКСвоимВоротам (Hockeyist self, World world, Com.CodeGame.CodeHockey2014.DevKit.CSharpCgdk.Model.Move move)
		{

			move.Action = ActionType.TakePuck;
					if (ВрагиБлизко(self, world, g_game.StickLength*0.5D)) { 
						move.Action = ActionType.Strike;
						move.SpeedUp = 0.0D;
						Сообщить("Драка"); }else{

				ИдтиКЦели(self, move, ЦентрПоляХ, УдачноеY);}

		}

		private bool ПопробоватьПасс(Hockeyist self, World world, Move move)
		{
			if (ВрагиБлизко(self, world, g_game.StickLength))
				if (ДатьПасс(self, world, move)) return true;
			ЗанятьУдачнуюПозициюНаПоле(self, world, move);
			return false;
		}

		private bool НахожусьВообщеДалеко(Hockeyist self)
		{
			return Math.Abs(self.X - netX) > Math.Abs(ЦентрПоляХ - netX);
		}

		private bool НахожусьВУдачномXМесте(Hockeyist self)
		{
			return (self.X < УдачноеX + УдачныйРадиусX)
				&& (self.X > УдачноеX - УдачныйРадиусX)
					;

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
			УдачныйРадиусY = world.Width/6.0D;
			УдачноеX = (ЦентрПоляХ + opponentPlayer.NetFront) * 0.5D;
			УдачноеX = (ЦентрПоляХ + УдачноеX) * 0.5D;
			УдачныйРадиусX = Math.Abs(world.Width/12.0D);
			//УдачноеX = (УдачноеX + ЦентрПоляХ) * 0.5D;
		}

		private void Сообщить(string Строка)
		{
			//if ((w_orld.Tick & 1) == 1)
			System.Console.Out.WriteLine(s_elf.Id.ToString() + " " + Строка + "  " + w_orld.Tick.ToString());
		}

		private bool ДатьПасс(Hockeyist self, World world, Model.Move move)
		{

			Hockeyist ПринимающийИгрок = ИгрокГотовПринять(self, world, move);
			if (ПринимающийИгрок == null) return false;
			move.PassPower = world.Height / self.GetDistanceTo(ПринимающийИгрок);
			if(move.PassPower < 0.2D)move.PassPower =1.0D;
			double Угол =  self.GetAngleTo(ПринимающийИгрок);
			move.Turn = Угол;
			move.PassAngle = Угол;

			if (Math.Abs(Угол) < STRIKE_ANGLE*5.0D)	{
				move.Action = ActionType.Pass;
			Сообщить("Пассую " + ПринимающийИгрок.Id.ToString());
			}
			return true;
		}

		private Hockeyist ИгрокГотовПринять(Hockeyist self, World world, Model.Move move)
		{

			var ИщемСвоего = from Hockeyist игрок in world.Hockeyists where 
				игрок.Id != self.Id && игрок.IsTeammate 
					&& игрок.Type != HockeyistType.Goalie 
					&& игрок.State == HockeyistState.Active 
					&& НахожусьВУдачномXМесте(игрок) 
					&& !ВрагиБлизко(игрок, world, world.Puck.Radius  + игрок.Radius  + g_game.StickLength)
				//&& КтоНаЛинииОгня(self, world, игрок.X, игрок.Y) == null 
					select игрок;
			return ИщемСвоего.FirstOrDefault();
		}

		private bool УдачныйМомент(Hockeyist self, World world)
		{
			ПолучитьТочкуУдараПоВоротам(self, world, out netX, out netY);
			return (КтоНаЛинииОгня(self, world, netX, netY) == null);
		}

		private Hockeyist КтоНаЛинииОгня(Hockeyist self, World world, double x, double y)
		{

			var Ищем = from Hockeyist игрок in world.Hockeyists where !игрок.IsTeammate && игрок.Id != self.Id && НаЛинии(self, x, y, игрок, world.Puck.Radius*0.5D) select игрок;
			Hockeyist ИгрокНаПути = Ищем.FirstOrDefault();
			if (ИгрокНаПути != null) Сообщить(ИгрокНаПути.Id.ToString() + " на пути");
			return ИгрокНаПути;

		}
		private IEnumerable<Hockeyist> КтоНаПути(Hockeyist self, World world, double x, double y, double r)
		{
			return from Hockeyist игрок in world.Hockeyists where !игрок.IsTeammate && self.GetDistanceTo(игрок) < r && НаЛинии(self, x, y, игрок, world.Puck.Radius*0.5D) select игрок;
		}

		private bool НаЛинии(Hockeyist self, double x, double y, Hockeyist игрок, double p)
		{
			double ty = self.Y + ((игрок.X - x) * (y - self.Y)) / (x - self.X);
			return Math.Abs(ty - игрок.Y) < игрок.Radius + p * 0.5D;
		}



		private void ЗанятьУдачнуюПозициюНаПоле(Hockeyist self, World world, Model.Move move)
		{
			move.Action = ActionType.TakePuck;
			bool МояШайба = ШайбаУМеня(self, world);
			if (!МояШайба)
			{
				if (ВрагиБлизко(self, world, g_game.StickLength*0.5D)) { 
					move.Action = ActionType.Strike;
					Сообщить("Драка"); }
				if(ШайбаУМоейКоманды(self,world))
				if(ШайбаНаМоейСторонеПоля(self,world)){
					if(НашихБьют(self,world,move))
						return;
				}

			}


			if (НахожусьВУдачномМесте(self))
			{
				double angleToNet = self.GetAngleTo(netX, netY);
				move.Turn = angleToNet;
				move.SpeedUp = -0.5D;
				if (ВрагиБлизко(self, world, self.Radius)) move.SpeedUp = -1.0D;
			}
			else
			{
				if (МояШайба) {
					if (УбежалОтВрагов(self,world,move))
						ИдтиКЦели(self, move, УдачноеX, УдачноеY);
				}
				else
				{
					ИдтиКЦели(self, move, УдачноеX, УдачноеY);
				}
			}
		}

		private bool НашихБьют(Hockeyist self, World world, Model.Move move){

			Hockeyist Мой = (from Hockeyist Owner in world.Hockeyists where
			                 Owner.Id == world.Puck.OwnerHockeyistId select Owner).FirstOrDefault();
			if(Мой == null) return false;

			Hockeyist ПристаетКМоему = (from Hockeyist Враг in world.Hockeyists where Враг.GetDistanceTo(Мой) < g_game.StickLength select Враг).FirstOrDefault();
			if(ПристаетКМоему == null) return false;
			ИдтиКЦели(self, move, ПристаетКМоему.X, ПристаетКМоему.Y);
			return true;

		}

		private bool ШайбаНаМоейСторонеПоля(Hockeyist self, World world){

			return Math.Sign(world.Puck.X - ЦентрПоляХ) == Math.Sign(ЦентрПоляХ - УдачноеX);

		}

		private bool УбежалОтВрагов(Hockeyist self, World world, Model.Move move)
		{
			double r = 2.0D * world.Puck.Radius +  g_game.StickLength;

			var ВрагиКругом = from Hockeyist игрок in world.Hockeyists where !игрок.IsTeammate 
				&& self.GetDistanceTo(игрок) < (r)
					&& Math.Abs(self.GetAngleTo(игрок)) < 0.3D
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
			//ЗажалиВУглу (self, world, ref Fangle);
			ИдтиТуда(move, Fangle);
			Сообщить("Убегаю");
			return false;
		}

		private void ЗажалиВУглу (Hockeyist self, World world, ref double Fangle)
		{
			double Дальность = 3.0D;
			if ((self.X + self.Radius + self.SpeedX * Дальность) >= world.Width)
				Fangle -= 0.5D;
			if ((self.X - self.Radius + self.SpeedX * Дальность) <= 0.0D)
				Fangle += 0.5D;
			if ((self.Y + self.Radius + self.SpeedY * Дальность) >= world.Height)
				Fangle -= 0.5D;
			if ((self.Y - self.Radius + self.SpeedY * Дальность) <= 0.0D)
				Fangle += 0.5D;
		}

		private void ИдтиКЦели(Hockeyist self, Model.Move move, double УдачноеX, double УдачноеY)
		{
			double Fangle = self.GetAngleTo(УдачноеX, УдачноеY);
			//Сообщить("Угол " + Fangle.ToString());
			ИдтиТуда(move, Fangle);
		}

		private void ИдтиТуда(Model.Move move, double Fangle)
		{

			if (Math.Abs(Fangle) > Math.PI * 0.93D)
			{

//				move.Turn = (!ШайбаУМеня(s_elf, w_orld)) ? -Fangle : Fangle;
				move.Turn =  -Fangle ;
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
				y = (Вратарь.Y < СерединаВорот) ? opponentPlayer.NetBottom  : opponentPlayer.NetTop  ;
			}
			else
			{
				y = (0.5D * (opponentPlayer.NetBottom + opponentPlayer.NetTop));
			}
		}

		private void ДогнатьШайбу(Hockeyist self, World world, Move move)
		{	double dist = self.GetDistanceTo(world.Puck);
			double РазностьУглов = Math.Abs(world.Puck.GetAngleTo(self));
			double КоэфициентОпереженияШайбы = РазностьУглов * 10.0D;
			double КоэфициентОпережения = dist/world.Height * world.Puck.Mass * КоэфициентОпереженияШайбы;
			if(dist<(world.Puck.Radius+self.Radius))КоэфициентОпережения = 1.0D;
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

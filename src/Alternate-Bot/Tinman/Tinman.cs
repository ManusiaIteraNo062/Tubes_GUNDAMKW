using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Tinman : Bot
{
    private const double EffectiveFiringRange = 300; 
    private const double MeleePocketDistance = 50;   
    private const double PointBlankMaxRange = 50;    

    private double targetX = -1;
    private double targetY = -1;
    private bool trackingTarget = false;
    private int trackScanTicks = 0;

    private bool jitterDirection = true; 
    private double currentJitterRange = 12.0; 
    private int hitReactionCooldown = 0;

    static void Main(string[] args)
    {
        new Tinman().Start();
    }

    public Tinman() : base(BotInfo.FromFile("Tinman.json")) { }

    public override void Run()
    {
        BodyColor   = Color.FromArgb(0x8B, 0x45, 0x13); 
        TurretColor = Color.FromArgb(0xC0, 0xC0, 0xC0); 
        RadarColor  = Color.FromArgb(0x70, 0x80, 0x90); 
        BulletColor = Color.FromArgb(0x8B, 0x00, 0x00); 
        ScanColor   = Color.FromArgb(0xA9, 0xA9, 0xA9); 

        while (IsRunning)
        {
            double distToLeft   = X;
            double distToRight  = ArenaWidth - X;
            double distToBottom = Y;
            double distToTop    = ArenaHeight - Y;
            double closestWall  = Math.Min(Math.Min(distToLeft, distToRight), Math.Min(distToBottom, distToTop));

            if (hitReactionCooldown > 0)
            {
                hitReactionCooldown--;
                if (hitReactionCooldown == 0) currentJitterRange = 12.0;
            }

            if (!trackingTarget)
            {
                if (closestWall < 45)
                {
                    TurnLeft(90);   
                    Forward(80);    
                    continue; 
                }

                double jitterMove = jitterDirection ? currentJitterRange : -currentJitterRange;
                SetForward(jitterMove);
                jitterDirection = !jitterDirection;
            }
            else
            {
                double currentDistance = DistanceTo(targetX, targetY);
                double angleToTarget = DirectionTo(targetX, targetY);

                if (currentDistance > MeleePocketDistance)
                {
                    if (closestWall < 45)
                    {
                        TurnLeft(90);   
                        Forward(80);    
                        continue; 
                    }

                    double steeringBearing = NormalizeRelativeAngle(angleToTarget - Direction);

                    if (hitReactionCooldown > 18)
                    {
                        steeringBearing = NormalizeRelativeAngle((angleToTarget - 35) - Direction);
                    }
                    else if (hitReactionCooldown > 0)
                    {
                        steeringBearing = NormalizeRelativeAngle((angleToTarget + 35) - Direction);
                    }

                    SetTurnLeft(steeringBearing);
                    SetForward(120); 
                }
                else
                {
                    if (closestWall < 40)
                    {
                        double steeringBearing = NormalizeRelativeAngle(angleToTarget - Direction);
                        SetTurnLeft(steeringBearing);
                        SetForward(-20);
                    }
                    else
                    {
                        double steeringBearing = NormalizeRelativeAngle((angleToTarget + 90) - Direction);
                        SetTurnLeft(steeringBearing);

                        double orbitMove = jitterDirection ? 8.0 : -8.0; 
                        SetForward(orbitMove);
                        jitterDirection = !jitterDirection;
                    }
                }
            }

            if (!trackingTarget)
            {
                SetTurnRadarLeft(360);
            }
            else
            {
                double absoluteDirection = DirectionTo(targetX, targetY);
                double currentDistance = DistanceTo(targetX, targetY);

                double gunBearing = NormalizeRelativeAngle(absoluteDirection - GunDirection);
                SetTurnGunLeft(gunBearing);

                double radarBearing = NormalizeRelativeAngle(absoluteDirection - RadarDirection);
                double microSweep = (trackScanTicks % 2 == 0) ? 6 : -6;
                SetTurnRadarLeft(radarBearing + microSweep);

                if (currentDistance <= EffectiveFiringRange)
                {
                    double bulletPower = (currentDistance <= PointBlankMaxRange) ? 3.0 : 1.2;
                    Fire(bulletPower);
                }

                trackScanTicks++;
                if (trackScanTicks >= 20)
                {
                    trackingTarget = false;
                    trackScanTicks = 0;
                }
            }

            Go();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        targetX = e.X;
        targetY = e.Y;
        trackingTarget = true;
        trackScanTicks = 0;
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        hitReactionCooldown = 35; 
    }
}
//using System.Collections.Generic;
//using OpenTK.Mathematics;

//namespace Lab2
//{
//    public static class Physics
//    {
//        public static void HandleCollisions(List<BilliardBall> balls)
//        {
//            for (int i = 0; i < balls.Count; i++)
//            {
//                for (int j = i + 1; j < balls.Count; j++)
//                {
//                    var ball1 = balls[i];
//                    var ball2 = balls[j];

//                    Vector3 delta = ball2.Position - ball1.Position;
//                    float distance = delta.Length;
//                    float minDistance = ball1.Radius + ball2.Radius;

//                    if (distance < minDistance)
//                    {
//                        Vector3 normal = delta.Normalized();
//                        Vector3 relativeVelocity = ball1.Velocity - ball2.Velocity;
//                        float velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);

//                        if (velocityAlongNormal > 0) continue;

//                        float restitution = 0.9f;
//                        float impulseMagnitude = -(1 + restitution) * velocityAlongNormal / 2;
//                        Vector3 impulse = impulseMagnitude * normal;

//                        ball1.Velocity += impulse;
//                        ball2.Velocity -= impulse;

//                        float penetration = minDistance - distance;
//                        Vector3 correction = normal * (penetration / 2);
//                        ball1.Position -= correction;
//                        ball2.Position += correction;
//                    }
//                }
//            }
//        }
//    }
//}

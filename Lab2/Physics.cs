using System.Collections.Generic;
using OpenTK.Mathematics;

public class Physics
{
    public class Ball
    {
        public Vector3 Position;
        public Vector3 Velocity;
        public float Radius = 0.057f;
        public bool IsActive = true;

        public Ball(Vector3 position)
        {
            Position = position;
            Velocity = Vector3.Zero;
        }

        public void Update(float deltaTime, float friction)
        {
            if (!IsActive) return;

            Position += Velocity * deltaTime;
            Velocity *= friction;

            if (Velocity.Length < 0.01f)
                Velocity = Vector3.Zero;
        }
    }

    private List<Ball> balls;
    private float friction = 0.98f;
    private float tableWidth = 1.5f;
    private float tableHeight = 3.0f;
    private float pocketRadius = 0.1f;

    public Physics(List<(Vector3 position, Vector3 color)> ballData)
    {
        balls = new List<Ball>();
        foreach (var (position, _) in ballData)
        {
            balls.Add(new Ball(position));
        }
    }

    public List<Ball> GetBalls()
    {
        return balls;
    }

    public void Update(float deltaTime)
    {
        foreach (var ball in balls)
        {
            ball.Update(deltaTime, friction);
            HandleWallCollision(ball);
            HandlePocket(ball);
        }

        HandleBallCollisions();
    }

    private void HandleWallCollision(Ball ball)
    {
        if (!ball.IsActive) return;

        if (ball.Position.X < -tableWidth + ball.Radius || ball.Position.X > tableWidth - ball.Radius)
        {
            ball.Velocity.X = -ball.Velocity.X;
            ball.Position.X = MathHelper.Clamp(ball.Position.X, -tableWidth + ball.Radius, tableWidth - ball.Radius);
        }

        if (ball.Position.Z < -tableHeight + ball.Radius || ball.Position.Z > tableHeight - ball.Radius)
        {
            ball.Velocity.Z = -ball.Velocity.Z;
            ball.Position.Z = MathHelper.Clamp(ball.Position.Z, -tableHeight + ball.Radius, tableHeight - ball.Radius);
        }
    }

    private void HandlePocket(Ball ball)
    {
        if (!ball.IsActive) return;

        var pockets = new List<Vector3>
        {
            new Vector3(-tableWidth, 0, -tableHeight),
            new Vector3(0, 0, -tableHeight),
            new Vector3(tableWidth, 0, -tableHeight),
            new Vector3(-tableWidth, 0, tableHeight),
            new Vector3(0, 0, tableHeight),
            new Vector3(tableWidth, 0, tableHeight),
        };

        foreach (var pocket in pockets)
        {
            if ((ball.Position - pocket).Length < pocketRadius)
            {
                ball.IsActive = false;
                ball.Velocity = Vector3.Zero;
            }
        }
    }

    private void HandleBallCollisions()
    {
        for (int i = 0; i < balls.Count; i++)
        {
            for (int j = i + 1; j < balls.Count; j++)
            {
                Ball a = balls[i];
                Ball b = balls[j];

                if (!a.IsActive || !b.IsActive)
                    continue;

                Vector3 delta = b.Position - a.Position;
                float distance = delta.Length;
                float minDistance = a.Radius + b.Radius;

                if (distance < minDistance && distance > 0.0001f)
                {
                    Vector3 normal = delta.Normalized();
                    Vector3 relativeVelocity = a.Velocity - b.Velocity;
                    float velocityAlongNormal = Vector3.Dot(relativeVelocity, normal);

                    if (velocityAlongNormal > 0)
                        continue;

                    float restitution = 1.0f;
                    float impulseScalar = -(1 + restitution) * velocityAlongNormal / 2;
                    Vector3 impulse = impulseScalar * normal;

                    a.Velocity += impulse;
                    b.Velocity -= impulse;

                    float penetration = minDistance - distance;
                    Vector3 correction = normal * penetration / 2;
                    a.Position -= correction;
                    b.Position += correction;
                }
            }
        }
    }

    public void StrikeBall(Vector3 direction)
    {
        if (balls.Count == 0) return;

        Ball cueBall = balls[0];
        if (!cueBall.IsActive) return;

        cueBall.Velocity += direction.Normalized() * 5.0f;
    }

    public void ResetBalls(List<(Vector3 position, Vector3 color)> ballData)
    {
        balls.Clear();
        foreach (var (position, _) in ballData)
        {
            balls.Add(new Ball(position));
        }
    }
}

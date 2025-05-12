using System.Collections.Generic;
using Lab2;
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
    private List<Vector3> pockets = new();

    public Physics(List<(Vector3 position, Vector3 color)> ballData)
    {
        balls = new List<Ball>();
        foreach (var (position, _) in ballData)
        {
            balls.Add(new Ball(position));
        }
    }

    public Physics(List<(Vector3 position, Vector3 color)> ballData, Table table)
    {
        balls = new List<Ball>();
        foreach (var (position, _) in ballData)
        {
            balls.Add(new Ball(position));
        }

        var (width, height) = table.GetTableSize();
        tableWidth = width;
        tableHeight = height;

        // Берём карманы из модели
        pockets = table.GetPocketPositions();

        // Радиус тоже из модели
        pocketRadius = table.GetPocketRadiusEstimate();

        Console.WriteLine($"Table size: width={tableWidth}, height={tableHeight}, pocket radius={pocketRadius}");
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

        bool collision = false;

        // Смещение для бортиков — примерно двойной радиус шара (чтобы шары не врезались в борта визуально)
        float borderOffset = ball.Radius * 2.0f;

        // Левая и правая стены
        float minX = -tableWidth + borderOffset + ball.Radius;
        float maxX = tableWidth - borderOffset - ball.Radius;

        if (ball.Position.X < minX)
        {
            ball.Position.X = minX;
            ball.Velocity.X = -ball.Velocity.X;
            collision = true;
        }
        else if (ball.Position.X > maxX)
        {
            ball.Position.X = maxX;
            ball.Velocity.X = -ball.Velocity.X;
            collision = true;
        }

        // Верхняя и нижняя стены (по Z)
        float minZ = -tableHeight + borderOffset + ball.Radius;
        float maxZ = tableHeight - borderOffset - ball.Radius;

        if (ball.Position.Z < minZ)
        {
            ball.Position.Z = minZ;
            ball.Velocity.Z = -ball.Velocity.Z;
            collision = true;
        }
        else if (ball.Position.Z > maxZ)
        {
            ball.Position.Z = maxZ;
            ball.Velocity.Z = -ball.Velocity.Z;
            collision = true;
        }

        if (collision)
        {
            ball.Velocity *= 0.9f;
        }
    }

    private void HandlePocket(Ball ball)
    {
        if (!ball.IsActive) return;

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

                    float restitution = 1.5f;
                    float impulseScalar = -(1 + restitution) * velocityAlongNormal / 2.0f;
                    Vector3 impulse = impulseScalar * normal;

                    a.Velocity += impulse;
                    b.Velocity -= impulse;

                    float penetration = minDistance - distance;
                    Vector3 correction = normal * (penetration / 2.0f);
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

        cueBall.Velocity += direction.Normalized() * 70.0f;
    }

    public void ResetBalls(List<(Vector3 position, Vector3 color)> ballData)
    {
        balls.Clear();
        foreach (var (position, _) in ballData)
        {
            balls.Add(new Ball(position));
        }
    }

    public void SetTableSize(float width, float height)
    {
        tableWidth = width / 2.0f;
        tableHeight = height / 2.0f;
    }

    public (float width, float height) GetTableSize()
    {
        return (tableWidth * 2.0f, tableHeight * 2.0f);
    }

    public static (float width, float height) GetTableSizeFromMesh(List<Vector3> meshVertices)
    {
        if (meshVertices == null || meshVertices.Count == 0)
            return (0, 0);

        float minX = meshVertices[0].X;
        float maxX = meshVertices[0].X;
        float minZ = meshVertices[0].Z;
        float maxZ = meshVertices[0].Z;

        foreach (var v in meshVertices)
        {
            if (v.X < minX) minX = v.X;
            if (v.X > maxX) maxX = v.X;
            if (v.Z < minZ) minZ = v.Z;
            if (v.Z > maxZ) maxZ = v.Z;
        }

        float width = maxX - minX;
        float height = maxZ - minZ;

        return (width, height);
    }
}

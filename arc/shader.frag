#version 330 core

in vec2 TexCoord;
in vec3 Normal;
in vec3 FragPos;

uniform sampler2D texture_diffuse;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform vec3 color;

out vec4 FragColor;

void main()
{
    vec3 baseColor = texture(texture_diffuse, TexCoord).rgb;

    // Если текстура отсутствует — используем переданный цвет
    if (baseColor == vec3(0.0)) {
        baseColor = color;
    }

    vec3 lightDir = normalize(lightPos - FragPos);
    float diff = max(dot(Normal, lightDir), 0.0);
    vec3 diffuse = diff * baseColor;

    FragColor = vec4(diffuse, 1.0);
}

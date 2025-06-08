//Этот вершинный шейдер используется для источника света (маленького куба):

//Принимает только позиции вершин (aPos)

//Вычисляет итоговую позицию вершины с помощью матриц model, view и projection

//Более простой, так как светящийся куб не требует нормалей или текстур

#version 330 core
layout (location = 0) in vec3 aPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AICommand
{
    public enum Commands
    {
        move,
        attack
    }
    private Commands command;
    private CharacterUnit target;
    private int x;
    private int y;

    public Commands Command { get => command; set => command = value; }
    public CharacterUnit Target { get => target; set => target = value; }
    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }

    public AICommand(Commands command, CharacterUnit target)
    {
        this.Command = command;
        this.Target = target;
    }

    public AICommand(Commands command, int x, int y)
    {
        this.Command = command;
        this.X = x;
        this.Y = y;
    }
}

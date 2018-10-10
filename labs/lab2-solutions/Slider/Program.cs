using System;

delegate void MoveEventHandler(object source, MoveEventArgs e);

public class MoveEventArgs : EventArgs
{
    public int newPosition;
    public bool cancel;
    public MoveEventArgs(int newPosition)
    {
        this.newPosition = newPosition;
    }
}

class Slider
{
    int position;
    public event MoveEventHandler Move;
    public int Position
    {
        get { return position; }
        set
        {
            if (position != value)
            { // if position changed
                if (Move != null)
                { // if invocation list not empty
                    MoveEventArgs args = new MoveEventArgs(value);
                    Move(this, args); // fire event
                    if (args.cancel)
                        return;
                }
                position = value;
            }
        }
    }
}

class Form
{
    static void Main()
    {
        Slider slider = new Slider();
        // register with the Move event
        slider.Move += new MoveEventHandler(slider_Move);
        slider.Position = 20;
        slider.Position = 60;
        Console.ReadLine();
    }
    
    
    static void slider_Move(object source, MoveEventArgs e)
    {
        if (e.newPosition < 50)
            Console.WriteLine("OK");
        else
        {
            e.cancel = true;
            Console.WriteLine("Can't go that high!");
        }
    }
}

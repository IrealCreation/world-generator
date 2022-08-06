
public class Notification
{
    public int Turn;
    public People People;
    public string Text;

    public Notification(People people, string text, int turn = 0)
    {
        Turn = (turn == 0 ? GameController.Main.Turn : turn);
        People = people;
        Text = text;
        
        GameController.Main.AddNotification(this);
    }
}
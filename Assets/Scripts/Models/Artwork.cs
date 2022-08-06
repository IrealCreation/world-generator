
public class Artwork
{
    /*
     * An artwork (and its name) is composed of three components, each one influencing an interest or ethos:
     * 1) the format, which depends on the ArtMovement currently adopted by its people (can influence interest / ethos)
     * 2) the subject, randomly selected on a pool of subjects fed by the choices (can influence interest / ethos)
     * 3) the descriptive, randomly selected on possibilities linked to the people's situation: politics, geography... (can influence basically anything)
     */
    public string Name;
    public People People;
    public int Turn;
    public ArtMovement Movement;
    public string Form; // In case the artwork doesn't belong to a true movement, here is the name of the raw ArtForm 
    public ArtSubject Subject;
    public string Descriptive;
    public string Focus; // Focus of the descriptive

    public Artwork(People people, ArtMovement movement, string form, ArtSubject subject, string descriptive, string focus)
    {
        People = people;
        Turn = GameController.Main.Turn;
        Movement = movement;
        Form = form;
        Subject = subject;
        Descriptive = descriptive;
        Focus = focus;

        Name = Subject.Name + " " + Descriptive;
    }

    public new string ToString()
    {
        return "\"" + Name + "\", " + (Movement != null ? Movement.Name : Form);
    }

    public string GetMovementName()
    {
        if (Movement != null)
            return Movement.Name;
        else
            return Form;
    }
}
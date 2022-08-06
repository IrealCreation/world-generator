
using System.Collections.Generic;

public class Choice
{
    public string Name;
    public string Description;
    public List<ChoiceOption> Options;

    public Choice(string name, string description, List<ChoiceOption> options)
    {
        Name = name;
        Description = description;
        Options = options;
    }
}
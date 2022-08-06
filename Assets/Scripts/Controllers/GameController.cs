
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Main;

    public InputController InputController;
    public MapUIController MapUIController;
    public ScreenUIController ScreenUIController;

    public HexMap HexMap;
    public GameObject HoverTextPopup;
    
    private List<People> peoples;
    private List<Notification> notifications;
    private Dictionary<string, Choice> choices;
    private Dictionary<string, Building> buildings;
    public List<Building> StartingBuildings; // List of the buildings the peoples can build from the start
    private Dictionary<string, Research> researchList;
    private List<ResearchEra> researchTree;
    private int researchEraIndex;
    
    private int currentPeopleID;
    public int Turn { get; private set; }
    private bool firstStart;
    public bool Omniscience; // Can we see everything?
    public bool Pause; // Is the time automatically pause?

    public void Start()
    {
        GameController.Main = this;
        firstStart = true;
        Omniscience = true;
        HexMap.Instantiate();
        StartGame();
        
        // Instantiate dictionnaries
        InstantiateChoices();
        InstantiateBuildings();
        InstantiateResearch();
    }

    public void StartGame(int seed = 0)
    {
        // Create empty dictionaries
        peoples = new List<People>();
        notifications = new List<Notification>();

        // In case this isn't our first start...
        if (!firstStart)
        {
            // Resets the state of the MapUIController
            MapUIController.Restart();
            
            // Remove the previous notifications
            ScreenUIController.RemoveNotifications();

            // Resets the state of the HexMap
            HexMap.DestroyMap();
        }

        // Generate the map
        if(seed != 0)
            HexMap.GenerateMap(seed);
        else
            HexMap.GenerateMap();
        HexMap.UpdateHexesVisuals();
        
        currentPeopleID = 0;
        firstStart = false;
    }

    public void NextTurn()
    {
        InputController.SelectedUnit = null;
        InputController.SelectedCity = null;
        
        foreach (People people in peoples)
        {
            people.EndTurn();
        }
        
        foreach (People people in peoples)
        {
            people.StartTurn();
        }
        
        ScreenUIController.UpdatePeopleInfos();
    }

    public void InstantiateChoices()
    {
        choices = new Dictionary<string, Choice>();
        choices.Add("cradle tropical", new Choice("Berceau tropical", 
            "C'est sous un climat tropical moite et humide que le peuple de $name a trouvé son berceau. Quel impact ces racines auront-elles sur ses traditions ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Que les jungles prolifiques lui fournissent une nourriture abondante", 
                    new Dictionary<string, int>() {{"food", 10}}),
                new ChoiceOption("Que les biens exotiques de ces latitudes lui apportent la prospérité", 
                    new Dictionary<string, int>() {{"wealth", 10}}),
                new ChoiceOption("Que la nature foisonnante guide les recherches de ses savants", 
                    new Dictionary<string, int>() {{"science", 10}}),
            }));
        choices.Add("cradle savanna", new Choice("Berceau savanne", 
            "C'est sous un chaud climat de savanne que le peuple de $name a trouvé son berceau. Quel impact ces racines auront-elles sur ses traditions ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Que la faune et la flore abondante soient propice à sa croissance", 
                    new Dictionary<string, int>() {{"food", 10}}),
                new ChoiceOption("Que ces immenses espaces incitent au commerce", 
                    new Dictionary<string, int>() {{"wealth", 10}}),
                new ChoiceOption("Que les prédateurs de cette faune soient un modèle pour ses guerriers", 
                    new Dictionary<string, int>() {{"military", 10}}),
            }));
        choices.Add("cradle desert", new Choice("Berceau désertique", 
            "C'est dans les déserts secs et brûlants que le peuple de $name a trouvé son berceau. Quel impact ces racines auront-elles sur ses traditions ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Que les caravanes fendent les dunes pour amener la richesse", 
                    new Dictionary<string, int>() {{"wealth", 10}}),
                new ChoiceOption("Que le soleil endurcisse ses combattants et les rende redoutables", 
                    new Dictionary<string, int>() {{"military", 10}}),
                new ChoiceOption("Que ses oasis de civilisation au milieu du désert attirent de loin les artistes", 
                    new Dictionary<string, int>() {{"culture", 10}}),
            }));
        choices.Add("cradle steppe", new Choice("Berceau steppe", 
            "C'est dans les steppes valonnées et arides que le peuple de $name a trouvé son berceau. Quel impact ces racines auront-elles sur ses traditions ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Que sa richesse soit bâtie sur les trésors enfouis dans la roche", 
                    new Dictionary<string, int>() {{"wealth", 10}}),
                new ChoiceOption("Que ses hordes conquérantes déferlent sans pitié depuis les hauteurs", 
                    new Dictionary<string, int>() {{"military", 10}}),
                new ChoiceOption("Que l'adaptation à cette terre aride motive l'ingéniosité", 
                    new Dictionary<string, int>() {{"science", 10}}),
            }));
        choices.Add("cradle temperate", new Choice("Berceau tempéré", 
            "C'est dans les vertes prairies tempérées que le peuple de $name a trouvé son berceau. Quel impact ces racines auront-elles sur ses traditions ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Que ces terres fertiles nourissent une population nombreuse", 
                    new Dictionary<string, int>() {{"food", 10}}),
                new ChoiceOption("Que cet horizon clément et serein favorise les échanges", 
                    new Dictionary<string, int>() {{"wealth", 10}}),
                new ChoiceOption("Que la douceur de vivre de ces lieux soit propice à la création artistique", 
                    new Dictionary<string, int>() {{"culture", 10}}),
            }));
        choices.Add("cradle taiga", new Choice("Berceau taïga", 
            "C'est dans le froid climat de la taïga que le peuple de $name a trouvé son berceau. Quel impact ces racines auront-elles sur ses traditions ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Que la chasse et la pêche nourisse malgré tout un peuple prolifique", 
                    new Dictionary<string, int>(){{"food", 10}}),
                new ChoiceOption("Que la vie rude en ces terres cultive la bravoure", 
                    new Dictionary<string, int>() {{"military", 10}}),
                new ChoiceOption("Que la beauté d'une nature exigeante prodigue l'inspiration", 
                    new Dictionary<string, int>() {{"culture", 10}}),
            }));
        choices.Add("cradle tundra", new Choice("Berceau toundra", 
            "C'est dans l'environnement glacial de la toundra que le peuple de $name a trouvé son berceau. Quel impact ces racines auront-elles sur ses traditions ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Que seuls les plus forts survivent en ce climat intransigeant", 
                    new Dictionary<string, int>() {{"military", 10}}),
                new ChoiceOption("Que la pureté intouchée des glaciers transcende ses artistes", 
                    new Dictionary<string, int>() {{"culture", 10}}),
                new ChoiceOption("Que ces conditions extrêmes ouvrent la voie à une meilleure compréhension du monde", 
                    new Dictionary<string, int>() {{"science", 10}}),
            }));
        
        //TODO: incomplet (trop redondant avec les recherches du néolithique)
        choices.Add("artisan circle", new Choice("Artisanat préhistorique",
            "Le fait de transformer des objets et de créer des outils est l'une des caractéristiques qui a permis à l'être humain de se distinguer. " +
            "Parmi ceux qui forment $name, quelle objet d'artisanat sera le mieux maîtrisé ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Un propulseur qui accompagnera les lances des chasseurs",
                    new Dictionary<string, int>() {{"food", 5}, {"military", 5}}),
            }));
        
        choices.Add("hunt camp", new Choice("Camp de chasse",
            "Les chasseurs de $name se regroupent dans leur campement, prêts à partir en de dangereuses expéditions dont ils espèrent revenir avec la viande " +
            "pour nourrir leur tribu, et forts d'une expérience guerrière qui pourra un jour leur être utile pour la protéger. Que doivent-ils avoir à l'esprit avant " +
            "de se mettre en chasse ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Qu'ils pensent à payer hommage à l'esprit des animaux tués pour les remercier de ce don de leur chair",
                    new Dictionary<string, int>() {{"religion", 5}}),
                new ChoiceOption("Essayer de capturer certains animaux : nous pourrons ainsi mieux les observer et leur trouver une utilité",
                    new Dictionary<string, int>() {{"wealth", 5}, {"science", 5}}),
                new ChoiceOption("Des chants pour accompagner la marche renforceront la camaraderie entre frères de chasse",
                    new Dictionary<string, int>() {{"culture", 5}, {"equality", 5}}),
                new ChoiceOption("Exalter leur instinct prédateur : c'est tuer ou être tué, seuls les meilleurs survivent",
                    new Dictionary<string, int>() {{"military", 5}, {"elitism", 5}, {"warmongering", 3}}),
                new ChoiceOption("Pas de risques inutiles, leurs familles attendent leur retour",
                    new Dictionary<string, int>() {{"food", 5}, {"warmongering", -3}}),
            }));
        
        choices.Add("fire pit", new Choice("Autour du feu",
            "La domestication du feu a été une étape importante de l'évolution humaine. Les flammes brûlantes et carnassières sont passées de danger imprévisible " +
            "à indispensable outil... En se regroupant autour du foyer et en regardant dans le coeur du brasier, qu'y voit la tribu de $name ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Les flammes éloigneront les bêtes sauvages et protégeront nos habitations",
                    new Dictionary<string, int>() {{"food", 5}, {"military", 5}}),
                new ChoiceOption("Cette chaleur permettra de travailler la pierre et de fondre les métaux",
                    new Dictionary<string, int>() {{"wealth", 5}, {"military", 5}}),
                new ChoiceOption("En nous permettant de cuire les aliments, le feu ouvre de nouvelles possibilités alimentaires",
                    new Dictionary<string, int>() {{"food", 5}, {"wealth", 5}}),
                new ChoiceOption("Ces flammes qui dansent devant nous sont sacrées, elles doivent être vénérées",
                    new Dictionary<string, int>() {{"culture", 5}, {"religion", 5}}),
                new ChoiceOption("La lumière ainsi prodiguée repousse l'obscurité et nous permet de mieux observer le monde",
                    new Dictionary<string, int>() {{"science", 5}, {"culture", 5}}),
                new ChoiceOption("Une fois le feu passé, tout redevient cendres : cela est une leçon sur notre humble place dans l'existence",
                    new Dictionary<string, int>() {{"science", 5}, {"religion", 5}}),
            }));
        
        choices.Add("totem", new Choice("Totémisme",
            "Tandis que les premières croyances de $name s'organisent autour de figures animistes inspirées de la nature qui les entoure, " +
            "quel sera l'animal-totem choisi par ce peuple ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Le loup",
                    new Dictionary<string, int>() {{"food", 5}, {"military", 3}, {"Taiga", 10}, {"Tundra", 10}}, new ArtSubject("Loup", "food")),
                new ChoiceOption("La truite",
                    new Dictionary<string, int>() {{"food", 5}, {"culture", 3}, {"Coast", 10}, {"Lake", 10}}, new ArtSubject("Truite", "food")),
                new ChoiceOption("Le renne",
                    new Dictionary<string, int>() {{"food", 5}, {"wealth", 3}, {"Taiga", 10}, {"Tundra", 10}}, new ArtSubject("Renne", "food")),
                new ChoiceOption("La gazelle",
                    new Dictionary<string, int>() {{"food", 5}, {"science", 3}, {"Desert", 10}, {"Savanna", 10}}, new ArtSubject("Gazelle", "food")),
                new ChoiceOption("L'écureuil",
                    new Dictionary<string, int>() {{"wealth", 5}, {"food", 3}, {"Temperate", 10}}, new ArtSubject("Ecureuil", "wealth")),
                new ChoiceOption("Le chameau",
                    new Dictionary<string, int>() {{"wealth", 5}, {"military", 3}, {"Desert", 10}}, new ArtSubject("Chameau", "wealth")),
                new ChoiceOption("Le lama",
                    new Dictionary<string, int>() {{"wealth", 5}, {"culture", 3}, {"Steppe", 10}}, new ArtSubject("Lama", "wealth")),
                new ChoiceOption("Le serpent",
                    new Dictionary<string, int>() {{"wealth", 5}, {"science", 3}, {"Desert", 10}, {"Tropical", 10}}, new ArtSubject("Serpent", "wealth")),
                new ChoiceOption("Le lion",
                    new Dictionary<string, int>() {{"military", 5}, {"culture", 3}, {"Savanna", 10}}, new ArtSubject("Lion", "military")),
                new ChoiceOption("Le cheval",
                    new Dictionary<string, int>() {{"military", 5}, {"wealth", 3}, {"Steppe", 10}}, new ArtSubject("Cheval", "military")),
                new ChoiceOption("Le taureau",
                    new Dictionary<string, int>() {{"military", 5}, {"food", 3}, {"Temperate", 10}}, new ArtSubject("Taureau", "military")),
                new ChoiceOption("L'aigle",
                    new Dictionary<string, int>() {{"military", 5}, {"science", 3}}, new ArtSubject("Aigle", "military")),
                new ChoiceOption("Le lynx",
                    new Dictionary<string, int>() {{"culture", 5}, {"science", 3}}, new ArtSubject("Lynx", "culture")),
                new ChoiceOption("Le coq",
                    new Dictionary<string, int>() {{"culture", 5}, {"food", 3}, {"Temperate", 10}}, new ArtSubject("Coq", "culture")),
                new ChoiceOption("Le perroquet",
                    new Dictionary<string, int>() {{"culture", 5}, {"wealth", 3}, {"Tropical", 10}}, new ArtSubject("Perroquet", "culture")),
                new ChoiceOption("L'éléphant",
                    new Dictionary<string, int>() {{"culture", 5}, {"military", 3}, {"Savanna", 10}, {"Tropical", 10}}, new ArtSubject("Eléphant", "culture")),
                new ChoiceOption("La chouette",
                    new Dictionary<string, int>() {{"science", 5}, {"culture", 3}}, new ArtSubject("Chouette", "science")),
                new ChoiceOption("Le dauphin",
                    new Dictionary<string, int>() {{"science", 5}, {"wealth", 3}, {"Coast", 10}}, new ArtSubject("Dauphin", "science")),
                new ChoiceOption("Le corbeau",
                    new Dictionary<string, int>() {{"science", 5}, {"military", 3}, {"Taiga", 10}}, new ArtSubject("Corbeau", "science")),
                new ChoiceOption("Le renard",
                    new Dictionary<string, int>() {{"science", 5}, {"food", 3}, {"Desert", 10}, {"Temperate", 10}}, new ArtSubject("Renard", "science")),
            }));
            
        choices.Add("farm", new Choice("Pratiques agricoles", 
            "La pratique de la cueillette comme source d'alimentation est progressivement remplacée par celle de l'agriculture, offrant un apport de " +
            "nourriture plus régulier et incitant à un mode de vie sédentaire. Les espèces végétales les plus propices à la culture sont petit à petit sélectionnées : " +
            "laquelle sera privilégiée par $name ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Le blé est assez polivalent, et sa farine se conserve bien", 
                    new Dictionary<string, int>() {{"Temperate", 10}, {"Taiga", 10}, {"food", 5}}),
                new ChoiceOption("Le seigle, bien que peu goutu, s'adapte aux terrains pauvres", 
                    new Dictionary<string, int>() {{"Temperate", 10}, {"Taiga", 10}, {"tundra", 10}, {"military", 5}}),
                new ChoiceOption("La pomme de terre est nourissante et bonne pour la santé", 
                    new Dictionary<string, int>() {{"Temperate", 10}, {"Taiga", 10}, {"science", 5}}),
                new ChoiceOption("Le millet peut pousser même en des conditions sèches et difficiles", 
                    new Dictionary<string, int>() {{"Desert", 10}, {"Steppe", 10}, {"military", 5}}),
                new ChoiceOption("Les dates croissent autour des oasis et peuvent être ensuite transportées", 
                    new Dictionary<string, int>() {{"Desert", 10}, {"Steppe", 10}, {"wealth", 5}}),
                new ChoiceOption("Les figues des régions arides ont un goût raffiné et apprécié", 
                    new Dictionary<string, int>() {{"Desert", 10}, {"Steppe", 10}, {"culture", 5}}),
                new ChoiceOption("Le riz peut avoir un excellent rendement en zone chaude et inondée", 
                    new Dictionary<string, int>() {{"Tropical", 10}, {"Savanna", 10}, {"food", 5}}),
                new ChoiceOption("Le maïs est une culture riche dans les climats humides", 
                    new Dictionary<string, int>() {{"Tropical", 10}, {"Savanna", 10}, {"wealth", 5}}),
                new ChoiceOption("Le taco est un tubercule tropical intéressant à cuisiner", 
                    new Dictionary<string, int>() {{"Tropical", 10}, {"Savanna", 10}, {"science", 5}}),
                new ChoiceOption("Les algues sont un choix particulier, mais aisées à cultiver le long des côtes", 
                    new Dictionary<string, int>() {{"Coast", 10}, {"Lake", 10}, {"culture", 5}}),
            }));
            
        choices.Add("husbandry", new Choice("Elevage", 
            "A la domestication des plantes a succédé celle des animaux ; plutôt que d'être chassés, certains sont devenus les compagnons des humains. " +
            "Lesquels seront apprivoisés au sein de $name ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Les chevaux seront de bonnes montures de combat sur terrain plat", 
                    new Dictionary<string, int>() {{"Plain", 10}, {"military", 5}},
                    new ArtSubject("Cheval", "military")),
                new ChoiceOption("Les moutons sont pourvus d'une fourrure qui peut avoir de la valeur", 
                    new Dictionary<string, int>() {{"Plain", 10}, {"wealth", 5}},
                    new ArtSubject("Mouton", "wealth")),
                new ChoiceOption("Les vaches peuvent nourrir par leur lait en plus de leur viande",
                    new Dictionary<string, int>() {{"Plain", 10}, {"food", 5}},
                    new ArtSubject("Taureau", "food")),
                new ChoiceOption("Les chiens sont les compagnons idéals du chasseur et du berger", 
                    new Dictionary<string, int>() {{"Plain", 10}, {"Hill", 10}, {"military", 5}},
                    new ArtSubject("Chien", "military")),
                new ChoiceOption("Les chats peuvent être hautains, mais ils sont redoutables contre les nuisibles", 
                    new Dictionary<string, int>() {{"Plain", 10}, {"Water", 10}, {"culture", 5}},
                    new ArtSubject("Chat", "culture")),
                new ChoiceOption("Les chèvres, agiles dans les montagnes, peuvent aussi nous fournir du lait",
                    new Dictionary<string, int>() {{"Hill", 10}, {"Mountain", 10}, {"food", 5}},
                    new ArtSubject("Bouc", "food")),
                new ChoiceOption("Les lamas ont une laine qui s'adapte au climat d'altitude",
                    new Dictionary<string, int>() {{"Hill", 10}, {"Mountain", 10}, {"wealth", 5}},
                    new ArtSubject("Lama", "wealth")),
                new ChoiceOption("Les saumons ont un cycle de reproduction particulier mais que nous pouvons peut-être maîtriser",
                    new Dictionary<string, int>() {{"Water", 10}, {"science", 5}},
                    new ArtSubject("Saumon", "science")),
                new ChoiceOption("Les carpes koï égaieront nos bassins de leurs fascinantes couleurs",
                    new Dictionary<string, int>() {{"Water", 10}, {"culture", 5}},
                    new ArtSubject("Carpe", "culture")),
                new ChoiceOption("Les pigeons pourraient avoir leur utilité en guise de messagers",
                    new Dictionary<string, int>() {{"Plain", 10}, {"Hill", 10}, {"science", 5}},
                    new ArtSubject("Oiseau", "science")),
            }));
            
        choices.Add("funerary rites", new Choice("Rites funéraires", 
            "Face à cet horizon inéluctable qu'est la mort, les habitants de $name entourent le décès de leurs proches de rites et de coutumes qui leurs sont propres...",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Que les morts soient inhumés en de petites tombes dans un cimetière partagé par la communauté", 
                    new Dictionary<string, int>() {{"equality", 10}, {"food", 10}}),
                new ChoiceOption("Que des tombeaux monumentaux et un embaumement raffiné honorent les vivants les plus glorieux après leur trépas", 
                    new Dictionary<string, int>() {{"elitism", 10}, {"wealth", 10}, {"culture", 10}}),
                new ChoiceOption("Que ceux qui nous ont quitté reçoivent un brasier à la hauteur de leur bravoure", 
                    new Dictionary<string, int>() {{"elitism", 10}, {"military", 10}}),
                new ChoiceOption("Que la mort soit accueillie dans le dépouillement des choses matérielles : les cadavres seront exposés aux bêtes sauvages", 
                    new Dictionary<string, int>() {{"equality", 10}, {"science", 10}}),
            }));
            
        choices.Add("chamanism", new Choice("Curiosité primordiale", 
            "Les chamans de $name sont parmi les premiers à essayer de comprendre le monde qui les entoure et à appréhender ses mystères. " +
            "Vers quoi doivent-ils se tourner en cette quête de connaissance ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Vers le monde végétal, capable de nourrir et de guérir", 
                    new Dictionary<string, int>() {{"food", 10}},
                    new ArtSubject("Arbre", "food")),
                new ChoiceOption("Vers le monde animal, dont la sauvagerie peut être domestiquée", 
                    new Dictionary<string, int>() {{"military", 10}},
                    new ArtSubject("Troupeau", "military")),
                new ChoiceOption("Vers le monde minéral, couvant de nombreuses richesses en son sein", 
                    new Dictionary<string, int>() {{"wealth", 10}},
                    new ArtSubject("Rocher", "wealth")),
                new ChoiceOption("Vers les hommes, et leur culture unique", 
                    new Dictionary<string, int>() {{"culture", 10}},
                    new ArtSubject("Silhouette humaine", "culture")),
                new ChoiceOption("Vers les cieux, pour ainsi tenter de comprendre les choses qui les dépassent", 
                    new Dictionary<string, int>() {{"science", 10}, {"religion", 10}},
                    new ArtSubject("Nuage", "science")),
            }));
            
        choices.Add("armure", new Choice("Le choix des armes", 
            "Les progrès techniques ont permis aux artisans de $name de produire des armes de plus en plus diverses, " +
            "aptes à subvenir aux différents besoins des combattants. Quelle arme sera privilégiée par ses guerriers ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("L'épée demeure l'instrument idéal pour découper rapidement son adversaire.", 
                    new Dictionary<string, int>() {{"mobility", 10}, {"warmongering", 3}},
                    new ArtSubject("Épéiste", "science")),
                new ChoiceOption("La lance, adaptée aux groupes de soldat en formation serrée.", 
                    new Dictionary<string, int>() {{"discipline", 10}, {"warmongering", -3}},
                    new ArtSubject("Lancier", "wealth")),
                new ChoiceOption("L'arc, car un bon ennemi est un ennemi qui n'a pas le loisir de s'approcher.", 
                    new Dictionary<string, int>() {{"defence", 10}, {"warmongering", -6}},
                    new ArtSubject("Archer", "food")),
                new ChoiceOption("La hache, capable de férocement percer armures et boucliers.", 
                    new Dictionary<string, int>() {{"offence", 10}, {"warmongering", 6}},
                    new ArtSubject("Berserker", "military")),
            }));
            
        choices.Add("cosmogony", new Choice("Panthéon divin", 
            "Les croyances religieuses de $name se développent en un panthéon de dieux de plus en plus nombreux et organisé. " +
            "Sous le patronage de quelle divinité le peuple doit-il se placer ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Le dieu de la guerre.", 
                    new Dictionary<string, int>() {{"military", 10}, {"warmongering", 5}}),
                new ChoiceOption("La déesse de la chasse.", 
                    new Dictionary<string, int>() {{"military", 10}, {"food", 10}}),
                new ChoiceOption("La déesse du foyer.", 
                    new Dictionary<string, int>() {{"food", 10}, {"expansionnism", -5}}),
                new ChoiceOption("La déesse de la fertilité.", 
                    new Dictionary<string, int>() {{"food", 10}}),
                new ChoiceOption("Le dieu des voyages.", 
                    new Dictionary<string, int>() {{"wealth", 10}, {"science", 10}, {"expansionnism", 5}}),
                new ChoiceOption("Le dieu des arts.", 
                    new Dictionary<string, int>() {{"culture", 10}}),
                new ChoiceOption("Le dieu des festivités.", 
                    new Dictionary<string, int>() {{"culture", 10}, {"wealth", 10}}),
                new ChoiceOption("La déesse de la sagesse.", 
                    new Dictionary<string, int>() {{"science", 10}}),
                new ChoiceOption("Le dieu du feu.", 
                    new Dictionary<string, int>() {{"wealth", 10}, {"military", 10}, {"science", 10}}),
                new ChoiceOption("La déesse de l'amour.", 
                    new Dictionary<string, int>() {{"culture", 10}, {"warmongering", -5}}),
            }));
            
        choices.Add("philosophy", new Choice("Philosophie", 
            "Tandis que le savoir scientifique de $name s'étoffe, chaque réponse semble amener de nouvelles questions... " +
            "des penseurs de plus en plus nombreux se penchent sur la question du sens de la vie, mais quelle école l'emportera ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("Moralisme : l'homme doit avant tout chercher à mener une vie digne et morale", 
                    new Dictionary<string, int>() {{"military", 10}, {"religion", 10}, {"authority", 10}}),
                new ChoiceOption("Hédonisme : le but de l'existence est la recherche du bonheur au travers des plaisirs sains", 
                    new Dictionary<string, int>() {{"culture", 10}, {"wealth", 10}, {"religion", -10}, {"equality", 10}}),
                new ChoiceOption("Idéalisme : c'est par la réflexion et l'éducation que l'on accède à la véritable essence transcendante des choses", 
                    new Dictionary<string, int>() {{"science", 10}, {"elitism", 10}, {"religion", 10}}),
                new ChoiceOption("Matérialisme : la vérité du monde se situe dans sa réalité sensible et peut être déchiffrée par l'observation", 
                    new Dictionary<string, int>() {{"wealth", 10}, {"military", 10}, {"religion", -10}, {"liberty", 10}}),
                new ChoiceOption("Confucianisme : l'harmonie de la vie en société requiert le respect de normes et valeurs", 
                    new Dictionary<string, int>() {{"culture", 10}, {"elitism", 10}, {"authority", 10}, {"progressivism", -5}}),
                new ChoiceOption("Taoïsme : la société est un cadre artificiel ; il faut rechercher l'équilibre en vivant conformément à sa nature", 
                    new Dictionary<string, int>() {{"food", 10}, {"liberty", 10}, {"progressivism", 5}}),
                new ChoiceOption("Yoga : par la méditation et l'ascèse s'accomplit l'unification du corps, de l'esprit et de la spiritualité", 
                    new Dictionary<string, int>() {{"food", 10}, {"religion", 10}, {"equality", 10}}),
            }));
            
        choices.Add("strategy", new Choice("Approche stratégique", 
            "La chose militaire devient de plus en plus complexe : les armées s'agrandissent, les soldats se spécialisent, les champs de bataille " +
            " voient s'opposer l'intellect autant que la bravoure... Quelle sera l'élément-clé de la stratégie militaire de $name ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("La mobilité fulgurante apportée par une cavalerie d'élite", 
                    new Dictionary<string, int>() {{"mobility", 10}, {"elitism", 10}}),
                new ChoiceOption("Une phalange de citoyens-soldats disciplinés et solidaires", 
                    new Dictionary<string, int>() {{"discipline", 10}, {"egality", 10}}),
                new ChoiceOption("Des fortifications solides garantissant la liberté des habitants", 
                    new Dictionary<string, int>() {{"defence", 10}, {"liberty", 10}, {"warmongering", -5}}),
                new ChoiceOption("Rien ne remplace un entraînement rigoureux exaltant l'esprit martial de nos guerriers", 
                    new Dictionary<string, int>() {{"offence", 10}, {"authority", 10}, {"warmongering", 5}}),
            }));
            
        choices.Add("amphitheater", new Choice("Genre théâtral", 
            "Les pièces de théâtre se multiplient en $name, attirant des foules de plus en plus nombreuses. Mais sous la performance artistique " +
            "se cache bien souvent un message d'ordre plus politique, dont le ton varie selon les genres et les auteurs... Lequel doit prédominer ?",
            new List<ChoiceOption>()
            {
                new ChoiceOption("La comédie permet de librement rire de tout, des faibles comme des puissants", 
                    new Dictionary<string, int>() {{"equality", 10}}),
                new ChoiceOption("La satire est un instrument aiguisé pour critiquer certains abus du pouvoir", 
                    new Dictionary<string, int>() {{"liberty", 10}}),
                new ChoiceOption("La tragédie fait comprendre aux spectateurs leur petitesse face à l'inélucatibilité du destin", 
                    new Dictionary<string, int>() {{"authority", 10}}),
                new ChoiceOption("Le dithyrambe rend hommage aux grands hommes et aux dieux sous une forme raffinée", 
                    new Dictionary<string, int>() {{"elitism", 10}, {"religion", 10}}),
            }));
    }

    public void InstantiateBuildings()
    {
        buildings = new Dictionary<string, Building>();
        int baseCost = 400;
        
        // Initial buildings
        buildings.Add("gatherers hut", new Building("Hutte des cueilleurs", baseCost * 1, new Yields(100)));
        buildings.Add("artisans circle", new Building("Cercle d'artisans", baseCost * 1, new Yields(0, 100)));
        buildings.Add("hunt camp", new Building("Camp de chasse", baseCost * 1, new Yields(0, 0, 100), choices["hunt camp"]));
        buildings.Add("fire pit", new Building("Puits de feu", baseCost * 1, new Yields(0, 0, 0, 100), choices["fire pit"]));
        buildings.Add("totem", new Building("Totem", baseCost * 1, new Yields(0, 0, 0, 0, 100), choices["totem"]));
        
        StartingBuildings = new List<Building>() { buildings["gatherers hut"], buildings["artisans circle"], 
            buildings["hunt camp"], buildings["fire pit"], buildings["totem"]};
        
        // Bronze age buildings
        buildings.Add("farm", new Building("Ferme", baseCost * 3, new Yields(3), choices["farm"]));
        buildings.Add("mine", new Building("Mine", baseCost * 3, new Yields(0, 3)));
        buildings.Add("training ground", new Building("Terrain d'entraînement", baseCost * 3, new Yields(0, 0, 300)));
        buildings.Add("library", new Building("Bibliothèque", baseCost * 3, new Yields(0, 0, 0, 300)));
        buildings.Add("monument", new Building("Monument", baseCost * 3, new Yields(0, 0, 0, 0, 300)));
        
        /* Bâtiments d'adaptation :
         Steppe : ferme en terrasse (food), apacheta (culture - religion)
         Désert : sietch (militaire), caravansérail (wealth)
         Lac : jardins flottants (food)
         Taïga : cercle de pierre (science - religion)
         */
        
        // Bronze age / Archaic wonders
        buildings.Add("suspended gardens", new Building("Jardins suspendus", baseCost * 8, new Yields(400, 0, 0, 400, 0),
            null, new List<Biome>() {HexMap.Biomes["Temperate"], HexMap.Biomes["Desert"]}, null, true));
        buildings.Add("pyramids", new Building("Pyramides", baseCost * 8, new Yields(0, 400, 0, 0, 400),
            null, new List<Biome>() {HexMap.Biomes["Steppe"], HexMap.Biomes["Desert"]}, null, true));
        buildings.Add("stone circle", new Building("Cercle de pierres", baseCost * 8, new Yields(0, 0, 400, 400, 0),
            null, new List<Biome>() {HexMap.Biomes["Steppe"], HexMap.Biomes["Taiga"], HexMap.Biomes["Tundra"]}, null, true));
        buildings.Add("chichen", new Building("Chichen", baseCost * 8, new Yields(400, 0, 0, 0, 400),
            null, new List<Biome>() {HexMap.Biomes["Savanna"], HexMap.Biomes["Tropical"]}, null, true));
        buildings.Add("cothon", new Building("Cothon", baseCost * 8, new Yields(0, 400, 400, 0, 400),
            null, new List<Biome>() {HexMap.Biomes["Coast"]}, null, true));
        
        // Classical buildings
        buildings.Add("domain", new Building("Domaine", baseCost * 5, new Yields(500)));
        buildings.Add("market", new Building("Marché", baseCost * 5, new Yields(0, 500)));
        buildings.Add("watch tower", new Building("Tour de guet", baseCost * 5, new Yields(0, 0, 500)));
        buildings.Add("school", new Building("École", baseCost * 5, new Yields(0, 0, 0, 500)));
        buildings.Add("amphitheater", new Building("Amphithéâtre", baseCost * 5, new Yields(0, 0, 0, 0, 500), 
            choices["amphitheater"]));
        
        // Classical / Imperial wonders
        buildings.Add("parthenon", new Building("Parthénon", baseCost * 12, new Yields(0, 400, 0, 0, 800),
            null, null, null, true));
        buildings.Add("great library", new Building("Grande Bibliothèque", baseCost * 12, new Yields(0, 0, 0, 800, 400),
            null, null, null, true));
        buildings.Add("colossus", new Building("Colosse", baseCost * 12, new Yields(0, 800, 400, 0, 0),
            null, null, null, true));
        buildings.Add("terracota army", new Building("Armée de terre cuite", baseCost * 12, new Yields(400, 0, 800, 0, 0),
            null, null, null, true));
        // Machu Pichu, Colisée
        
        buildings.Add("fishing ships", new Building("Bâteaux pêcheurs", baseCost * 4, new Yields(200, 200)));
        
        buildings.Add("triremes", new Building("Trirèmes", baseCost * 6, new Yields(0, 300, 300)));
    }

    public void InstantiateResearch()
    {
        researchList = new Dictionary<string, Research>();
        researchTree = new List<ResearchEra>();
        
        // Paleolithic / Neolithic
        researchEraIndex = 0;
        researchTree.Add(new ResearchEra("Paléolithique", researchEraIndex));
        AddResearch("agriculture", new Research("Agriculture", researchTree[researchEraIndex], "food",
            null, null, null, null, new Dictionary<string, string>() {{"food", "Paysans"}}));
        AddResearch("potery", new Research("Poterie", researchTree[researchEraIndex], "wealth"));
        AddResearch("silex", new Research("Taille du silex", researchTree[researchEraIndex], "military"));
        AddResearch("funerary rites", new Research("Rites funéraires", researchTree[researchEraIndex], "science",
            null, null, choices["funerary rites"]));
        AddResearch("pigments", new Research("Pigments", researchTree[researchEraIndex], "culture"));
        AddResearch("husbandry", new Research("Elevage", researchTree[researchEraIndex], new List<string>() {"food", "wealth"},
            null, null, choices["husbandry"]));
        AddResearch("archery", new Research("Tir à l'arc", researchTree[researchEraIndex], new List<string>() {"food", "military"},
            null, null, null, "Propulseur taillé"));
        AddResearch("weaving", new Research("Tissage", researchTree[researchEraIndex], new List<string>() {"wealth", "science"},
            null, null, null, "Étoffe"));
        AddResearch("bone sculpting", new Research("Taille des os", researchTree[researchEraIndex], new List<string>() {"military", "culture"}, 
            null, null, null, "Sculpture sur os"));
        AddResearch("chamanism", new Research("Chamanisme", researchTree[researchEraIndex], new List<string>() {"science", "culture"},
            null, null, choices["chamanism"]));
        
        // Bronze age
        researchEraIndex ++;
        researchTree.Add(new ResearchEra("Age du bronze", researchEraIndex));
        AddResearch("irrigation", new Research("Irrigation", researchTree[researchEraIndex], "food",
            null, new List<Building>() {buildings["farm"]},
            null, null, new Dictionary<string, string>() {{"food", "Paysans"}}));
        AddResearch("bronze working", new Research("Travail du bronze", researchTree[researchEraIndex], "wealth",
            null, new List<Building>() {buildings["mine"]}));
        AddResearch("armour", new Research("Armure", researchTree[researchEraIndex], "military",
            null, new List<Building>() {buildings["training ground"]}, 
            choices["armure"], "Plastron", new Dictionary<string, string>() {{"military", "Guerriers"}}));
        AddResearch("writing", new Research("Ecriture", researchTree[researchEraIndex], "science",
            null, new List<Building>() {buildings["library"]}, 
            null, null, new Dictionary<string, string>() {{"science", "Scribes"}}));
        AddResearch("ornementation", new Research("Ornementation", researchTree[researchEraIndex], "culture",
            null, new List<Building>() {buildings["monument"]}, null, "Statuette"));
        AddResearch("wheel", new Research("Roue", researchTree[researchEraIndex], new List<string>() {"food", "military"}));
        AddResearch("code of laws", new Research("Code de loi", researchTree[researchEraIndex], new List<string>() {"food", "science"}));
        AddResearch("iron working", new Research("Travail du fer", researchTree[researchEraIndex], new List<string>() {"wealth", "military"}));
        AddResearch("gold working", new Research("Travail de l'or", researchTree[researchEraIndex], new List<string>() {"wealth", "culture"},
            null, null, null, "Bijou"));
        AddResearch("cosmogony", new Research("Cosmogonie", researchTree[researchEraIndex], new List<string>() {"science", "culture"}));
        
        // Archaic
        researchEraIndex ++;
        researchTree.Add(new ResearchEra("Antiquité archaïque", researchEraIndex));
        AddResearch("plough", new Research("Charue", researchTree[researchEraIndex], "food",
            null, null, null, null, new Dictionary<string, string>() {{"food", "Paysans"}}));
        AddResearch("coinage", new Research("Monnaie", researchTree[researchEraIndex], "wealth",
            null, null, null, "Pièce de monnaie"));
        AddResearch("formations", new Research("Formations", researchTree[researchEraIndex], "military",
            null, null, null, null, new Dictionary<string, string>() {{"military", "Guerriers"}}));
        AddResearch("philosophy", new Research("Philosophie", researchTree[researchEraIndex], "science",
            null, null, choices["philosophy"], null, new Dictionary<string, string>() {{"science", "Philosophes"}}));
        AddResearch("poetry", new Research("Poésie", researchTree[researchEraIndex], "culture",
            null, null, null, "Poésie"));
        AddResearch("navigation", new Research("Navigation", researchTree[researchEraIndex], new List<string>() {"food", "wealth"},
            null, new List<Building>() {buildings["fishing ships"]}));
        AddResearch("ceramic", new Research("Céramique", researchTree[researchEraIndex], new List<string>() {"food", "culture"},
            null, null, null, "Céramique"));
        AddResearch("mathematics", new Research("Mathématiques", researchTree[researchEraIndex], new List<string>() {"wealth", "science"}));
        AddResearch("poliorcetics", new Research("Poliorcétique", researchTree[researchEraIndex], new List<string>() {"military", "science"}));
        AddResearch("honor code", new Research("Code d'honneur", researchTree[researchEraIndex], new List<string>() {"military", "culture"}));
        
        // Classical
        researchEraIndex ++;
        researchTree.Add(new ResearchEra("Antiquité classique", researchEraIndex));
        AddResearch("mill", new Research("Moulin", researchTree[researchEraIndex], "food",
            null, new List<Building>() {buildings["domain"]}));
        AddResearch("trade", new Research("Commerce", researchTree[researchEraIndex], "wealth",
            null, new List<Building>() {buildings["market"]}));
        AddResearch("strategy", new Research("Stratégie", researchTree[researchEraIndex], "military",
            null, new List<Building>() {buildings["watch tower"]}, choices["strategy"], null, new Dictionary<string, string>() {{"military", "Soldats"}}));
        AddResearch("rhetorics", new Research("Rhétorique", researchTree[researchEraIndex], "science",
            null, new List<Building>() {buildings["school"]}, null, "Récit", new Dictionary<string, string>() {{"science", "Philosophes"}}));
        AddResearch("theater", new Research("Théâtre", researchTree[researchEraIndex], "culture",
            null, new List<Building>() {buildings["amphitheater"]}, null, "Théâtre"));
        AddResearch("constitution", new Research("Constitution", researchTree[researchEraIndex], new List<string>() {"food", "science"}));
        AddResearch("fortifications", new Research("Fortifications", researchTree[researchEraIndex], new List<string>() {"food", "military"}));
        AddResearch("warships", new Research("Vaisseaux de guerre", researchTree[researchEraIndex], new List<string>() {"wealth", "military"},
            null, new List<Building>() {buildings["triremes"]}));
        AddResearch("architecture", new Research("Architecture", researchTree[researchEraIndex], new List<string>() {"wealth", "culture"},
            null, null, null, "Bas-relief"));
        AddResearch("esthetics", new Research("Esthétique", researchTree[researchEraIndex], new List<string>() {"science", "culture"}));
        
        // Imperial
        researchEraIndex ++;
        researchTree.Add(new ResearchEra("Antiquité impériale", researchEraIndex));
        AddResearch("aqueducs", new Research("Aqueducs", researchTree[researchEraIndex], "food"));
        AddResearch("administration", new Research("Administration", researchTree[researchEraIndex], "wealth"));
        AddResearch("legions", new Research("Légions", researchTree[researchEraIndex], "military",
            null, null, null, null, new Dictionary<string, string>() {{"military", "Soldats"}}));
        AddResearch("codex", new Research("Codex", researchTree[researchEraIndex], "science",
            null, null, null, null, new Dictionary<string, string>() {{"science", "Philosophes"}}));
        AddResearch("glorification", new Research("Glorification", researchTree[researchEraIndex], "culture",
            null, null, null, "Statue"));
        AddResearch("monachism", new Research("Monachisme", researchTree[researchEraIndex], "religion"));
        AddResearch("medicine", new Research("Médecine", researchTree[researchEraIndex], new List<string>() {"food", "science"}));
        AddResearch("baths", new Research("Thermes", researchTree[researchEraIndex], new List<string>() {"food", "wealth"})); // Ou food-culture
        AddResearch("astrolabe", new Research("Astrolabe", researchTree[researchEraIndex], new List<string>() {"wealth", "science"}));
        AddResearch("triumph", new Research("Triomphe", researchTree[researchEraIndex], new List<string>() {"military", "culture"},
            null, null, null, "Arc de triomphe"));
    }

    public void AddResearch(string name, Research research)
    {
        researchList.Add(name, research);
        researchTree[researchEraIndex].Add(research);
    }

    public ResearchEra GetResearchEra(int index)
    {
        return researchTree[index];
    }

    public bool SpawnPeopleAt(string name, bool isPlayer, Hex hex)
    {
        // We cannot spawn on water or already claimed territory
        if (hex.Relief.IsWater || hex.People != null)
            return false;
        
        Color[] colors = new[]
        {
            Color.red, new Color(0, 0f, 1f), new Color(0, 0.9f, 0), Color.yellow, new Color(0.3f, 0, 0.8f), Color.cyan, 
            new Color(0.8f, 0.4f, 0), new Color(0.15f, 0.5f, 0.85f), new Color(0.75f, 0, 0.5f),
            new Color(0.4f, 0, 0), new Color(0.95f, 0.35f, 0.95f), new Color(1f, 0.35f, 0.35f)
        };
        String[] names = new[]
        {
            "Lorrez-le-Bocage", "Villebéon", "Nanteau-sur-Lunain", "Villemaugis", "Passy", "Chéroy",
            "Nemours", "Grez-sur-Loing", "Egreville", "Préaux", "Paley", "Tesnières"
        };
        if (peoples.Count >= colors.Length)
            return false;
        
        People people = new People(names[peoples.Count], colors[peoples.Count], isPlayer, HexMap, hex);
        peoples.Add(people);
        HexMap.SpawnPeopleAt(people, hex);
        PresentChoice(choices["cradle " + hex.Biome.Name.ToLower()], people);

        return true;
    }

    public People GetPeople(int id)
    {
        if (peoples.Count == 0)
            return null;
        return peoples[id];
    }
    public People CurrentPeople()
    {
        return GetPeople(currentPeopleID);
    }

    public void PresentChoice(Choice choice, People people)
    {
        if (people.IsPlayer)
        {
            // Check if no equivalent choice has already been made
            if (people.ChoicesMade.Keys.Contains(choice))
                return;

            Pause = true;
            ScreenUIController.ShowChoice(choice, people);
        }
        else
        {
            // Let the IA make the choice
            people.MakeChoiceAI(choice);
        }
    }

    public void MakeChoice(ChoiceOption option, Choice choice, People people)
    {
        ScreenUIController.HideChoice();
        people.MakeChoice(choice, option);
        Pause = false;
    }

    public void AddNotification(Notification notification)
    {
        notifications.Add(notification);
        Debug.Log(notification.People.Name + " - " + notification.Text);
        ScreenUIController.ShowNotification(notification);
    }
}
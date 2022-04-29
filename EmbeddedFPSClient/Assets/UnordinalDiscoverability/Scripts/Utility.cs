using UnityEngine;
using System.Net.NetworkInformation;
using System.IO;
using System;

namespace Unordinal.Discoverability
{
    public static class Utility
    {
        private const string resourcePathToGameId = "Unordinal/GameId";
        private const string filePathToGameId = "Assets/Resources/" + resourcePathToGameId + ".txt";
        
        private static string loadedGameId;
        private static readonly object lockObject = new object();

#if UNITY_EDITOR
        private static void GenerateGameId()
        {
            //store game ID that we create in memory, because a Unity editor restart is required
            //to actually initialize the resource asset properly with .meta
            loadedGameId = Guid.NewGuid().ToString();
            Debug.Log($"Game Id: {loadedGameId}.");

            if (!File.Exists("Assets/Resources"))
                Directory.CreateDirectory("Assets/Resources");
            if(!File.Exists("Assets/Resources/Unordinal"))
                Directory.CreateDirectory("Assets/Resources/Unordinal");
            
            File.WriteAllText(filePathToGameId, loadedGameId);
        }
#endif

        public static string LoadGameId()
        {
            lock (lockObject) //we don't know how user accesses this
            {
                if (loadedGameId == null)
                {
#if UNITY_EDITOR
                    if (!File.Exists(filePathToGameId))
                    {
                        Debug.Log("Discoverability: seems there is no Game Id for this project yet. I am generating one in: '" + filePathToGameId + "'. Find more information in the documentation.");
                        GenerateGameId();
                        return loadedGameId;
                    }
#endif

                    var asset = Resources.Load<TextAsset>(resourcePathToGameId);
                    if (asset != null)
                    {
                        loadedGameId = asset.text;
                    }
                    else
                    {
                        Debug.Log("Discoverability: seems there is no Game Id for this project, discoverability will not work without running once in Unity editor.");
                        return null;
                    }
                }

                return loadedGameId;
            }
        }

        internal static float PingServer(string serverURI)
        {
            System.Net.NetworkInformation.Ping ping = null;
            try
            {
                ping = new System.Net.NetworkInformation.Ping();
                PingReply reply = ping.Send(serverURI);
                return reply.RoundtripTime;
            }
            catch (PingException)
            {
                return -1;
            }
            finally
            {
                if (ping != null)
                    ping.Dispose();
            }
        }

        internal static class NameGenerator
        {
            private static string[] Colors =
            {
                "Aqua",
                "Aquamarine",
                "Azure",
                "Beige",
                "Bisque",
                "Black",
                "Blue",
                "Brown",
                "Chartreuse",
                "Chocolate",
                "Coral",
                "Cornsilk",
                "Crimson",
                "Cyan",
                "Fuchsia",
                "Gainsboro",
                "Gold",
                "Gray",
                "Grey",
                "Green",
                "Indigo",
                "Ivory",
                "Khaki",
                "Lavender",
                "Lime",
                "Linen",
                "Magenta",
                "Maroon",
                "Moccasin",
                "Navy",
                "Olive",
                "Orange",
                "Orchid",
                "Peru",
                "Pink",
                "Plum",
                "Purple",
                "Red",
                "Salmon",
                "Seashell",
                "Sienna",
                "Silver",
                "Snow",
                "Tan",
                "Teal",
                "Thistle",
                "Tomato",
                "Turquoise",
                "Violet",
                "Wheat",
                "White",
                "Yellow"
            };
            
            private static string[] Animals =
            {
                "Aardvark",
                "Albatross",
                "Alligator",
                "Alpaca",
                "Ant",
                "Anteater",
                "Antelope",
                "Ape",
                "Armadillo",
                "Donkey",
                "Baboon",
                "Badger",
                "Barracuda",
                "Bat",
                "Bear",
                "Beaver",
                "Bee",
                "Bison",
                "Boar",
                "Buffalo",
                "Butterfly",
                "Camel",
                "Capybara",
                "Caribou",
                "Cassowary",
                "Cat",
                "Caterpillar",
                "Cattle",
                "Chamois",
                "Cheetah",
                "Chicken",
                "Chimpanzee",
                "Chinchilla",
                "Chough",
                "Clam",
                "Cobra",
                "Cockroach",
                "Cod",
                "Cormorant",
                "Coyote",
                "Crab",
                "Crane",
                "Crocodile",
                "Crow",
                "Curlew",
                "Deer",
                "Dinosaur",
                "Dog",
                "Dogfish",
                "Dolphin",
                "Dotterel",
                "Dove",
                "Dragonfly",
                "Duck",
                "Dugong",
                "Dunlin",
                "Eagle",
                "Echidna",
                "Eel",
                "Eland",
                "Elephant",
                "Elk",
                "Emu",
                "Falcon",
                "Ferret",
                "Finch",
                "Fish",
                "Flamingo",
                "Fly",
                "Fox",
                "Frog",
                "Gaur",
                "Gazelle",
                "Gerbil",
                "Giraffe",
                "Gnat",
                "Gnu",
                "Goat",
                "Goldfinch",
                "Goldfish",
                "Goose",
                "Gorilla",
                "Goshawk",
                "Grasshopper",
                "Grouse",
                "Guanaco",
                "Gull",
                "Hamster",
                "Hare",
                "Hawk",
                "Hedgehog",
                "Heron",
                "Herring",
                "Hippopotamus",
                "Hornet",
                "Horse",
                "Human",
                "Hummingbird",
                "Hyena",
                "Ibex",
                "Ibis",
                "Jackal",
                "Jaguar",
                "Jay",
                "Jellyfish",
                "Kangaroo",
                "Kingfisher",
                "Koala",
                "Kookabura",
                "Kouprey",
                "Kudu",
                "Lapwing",
                "Lark",
                "Lemur",
                "Leopard",
                "Lion",
                "Llama",
                "Lobster",
                "Locust",
                "Loris",
                "Louse",
                "Lyrebird",
                "Magpie",
                "Mallard",
                "Manatee",
                "Mandrill",
                "Mantis",
                "Marten",
                "Meerkat",
                "Mink",
                "Mole",
                "Mongoose",
                "Monkey",
                "Moose",
                "Mosquito",
                "Mouse",
                "Mule",
                "Narwhal",
                "Newt",
                "Nightingale",
                "Octopus",
                "Okapi",
                "Opossum",
                "Oryx",
                "Ostrich",
                "Otter",
                "Owl",
                "Oyster",
                "Panther",
                "Parrot",
                "Partridge",
                "Peafowl",
                "Pelican",
                "Penguin",
                "Pheasant",
                "Pig",
                "Pigeon",
                "Pony",
                "Porcupine",
                "Porpoise",
                "Quail",
                "Quelea",
                "Quetzal",
                "Rabbit",
                "Raccoon",
                "Rail",
                "Ram",
                "Rat",
                "Raven",
                "Red deer",
                "Red panda",
                "Reindeer",
                "Rhinoceros",
                "Rook",
                "Salamander",
                "Salmon",
                "Sand Dollar",
                "Sandpiper",
                "Sardine",
                "Scorpion",
                "Seahorse",
                "Seal",
                "Shark",
                "Sheep",
                "Shrew",
                "Skunk",
                "Snail",
                "Snake",
                "Sparrow",
                "Spider",
                "Spoonbill",
                "Squid",
                "Squirrel",
                "Starling",
                "Stingray",
                "Stinkbug",
                "Stork",
                "Swallow",
                "Swan",
                "Tapir",
                "Tarsier",
                "Termite",
                "Tiger",
                "Toad",
                "Trout",
                "Turkey",
                "Turtle",
                "Viper",
                "Vulture",
                "Wallaby",
                "Walrus",
                "Wasp",
                "Weasel",
                "Whale",
                "Wildcat",
                "Wolf",
                "Wolverine",
                "Wombat",
                "Woodcock",
                "Woodpecker",
                "Worm",
                "Wren",
                "Yak",
                "Zebra"
            };

            internal static string GetRandomName()
            {
                string col = Colors[UnityEngine.Random.Range(0, Colors.Length)];
                string ani = Animals[UnityEngine.Random.Range(0, Animals.Length)];
                return col + " " + ani;
            }
        }
    }
}
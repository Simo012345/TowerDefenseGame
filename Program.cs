using System;
using System.Collections.Generic;
using System.IO;

// ==================== INTERFACES ====================
interface IAttaquable
{
    void RecevoirDegats(int degats);
}

interface IRalentissable
{
    void Ralentir(int valeur);
}

interface IAvancable
{
    void Avancer();
}

// ==================== CLASSE ABSTRAITE ENNEMI ====================
abstract class Ennemi : IAttaquable, IRalentissable, IAvancable
{
    public double Sante { get; set; }     // Setters publics (corrigé)
    public double Vitesse { get; set; }   // Setters publics (corrigé)
    public double Position { get; set; }  // Setters publics (corrigé)
    public string NomType { get; protected set; }

    public Ennemi(double sante, double vitesse, double position, string type)
    {
        Sante = sante;
        Vitesse = vitesse;
        Position = position;
        NomType = type;
    }

    public virtual void Avancer()
    {
        Position -= Vitesse;
    }

    public bool EstVivant()
    {
        return Sante > 0;
    }

    public abstract void RecevoirDegats(int degats);
    public abstract void Ralentir(int valeur);

    public override string ToString()
    {
        return $"{NomType} | Santé: {Sante:F1} | Vitesse: {Vitesse:F2} | Position: {Position:F1}";
    }
}

// ==================== CLASSES DÉRIVÉES D’ENNEMI ====================
class Gobelin : Ennemi
{
    public Gobelin(double position = 100) : base(30, 7, position, "Gobelin") { }

    public override void RecevoirDegats(int degats)
    {
        double total = degats * 2;
        Sante -= total;
        Console.WriteLine($"  -> Gobelin reçoit {total} dégâts (doublés). Nouvelle santé : {Sante:F1}");
    }

    public override void Ralentir(int valeur)
    {
        Vitesse = Math.Max(0, Vitesse - valeur);
        Console.WriteLine($"  -> Gobelin ralenti de {valeur}. Nouvelle vitesse : {Vitesse:F2}");
    }
}

class Orque : Ennemi
{
    public Orque(double position = 100) : base(60, 5, position, "Orque") { }

    public override void RecevoirDegats(int degats)
    {
        Sante -= degats;
        Console.WriteLine($"  -> Orque reçoit {degats} dégâts. Nouvelle santé : {Sante:F1}");
    }

    public override void Ralentir(int valeur)
    {
        Vitesse = Math.Max(0, Vitesse - valeur / 2.0);
        Console.WriteLine($"  -> Orque ralenti légèrement ({valeur / 2}). Nouvelle vitesse : {Vitesse:F2}");
    }
}

class Troll : Ennemi
{
    public Troll(double position = 100) : base(120, 3, position, "Troll") { }

    public override void RecevoirDegats(int degats)
    {
        double total = degats * 0.7;
        Sante -= total;
        Console.WriteLine($"  -> Troll reçoit {total:F1} dégâts (résistance 30%). Santé : {Sante:F1}");
    }

    public override void Ralentir(int valeur)
    {
        // Troll est immunisé
        Console.WriteLine($"  -> Troll est immunisé au ralentissement !");
    }
}

// ==================== TOURS ====================
abstract class Tour
{
    public int Degats { get; protected set; }
    public double Portee { get; protected set; }
    public string Nom { get; protected set; }

    public abstract void Attaquer(List<Ennemi> ennemis);
}

class TourDeFeu : Tour
{
    public TourDeFeu()
    {
        Nom = "Tour de Feu";
        Degats = 20;
        Portee = 50;
    }

    public override void Attaquer(List<Ennemi> ennemis)
    {
        foreach (var e in ennemis)
        {
            if (e.EstVivant() && e.Position <= Portee)
            {
                e.RecevoirDegats(Degats);
                double recul = 3;
                e.Position += recul; // Fonctionne maintenant (set public)
                Console.WriteLine($"  -> {e.NomType} est repoussé de {recul} !");
            }
        }
    }
}

class TourDeGlace : Tour
{
    public TourDeGlace()
    {
        Nom = "Tour de Glace";
        Degats = 10;
        Portee = 40;
    }

    public override void Attaquer(List<Ennemi> ennemis)
    {
        foreach (var e in ennemis)
        {
            if (e.EstVivant() && e.Position <= Portee)
            {
                e.RecevoirDegats(Degats);
                e.Ralentir(2);
            }
        }
    }
}

// ==================== JEU ====================
class Jeu
{
    private List<Ennemi> ennemis;
    private List<Tour> tours;
    private int tourActuel = 1;

    public Jeu()
    {
        ennemis = new List<Ennemi>();
        tours = new List<Tour>() { new TourDeFeu(), new TourDeGlace() };
    }

    public void Initialiser()
    {
        ennemis.Clear();
        ennemis.Add(new Gobelin());
        ennemis.Add(new Orque());
        ennemis.Add(new Troll());
    }

    public void Jouer()
    {
        Initialiser();

        while (ennemis.Exists(e => e.EstVivant()))
        {
            Console.WriteLine($"\n=== TOUR {tourActuel} ===");
            foreach (var t in tours)
            {
                Console.WriteLine($"Tour active : {t.Nom}");
                t.Attaquer(ennemis);
            }

            foreach (var e in ennemis)
                e.Avancer();

            AfficherEtat();

            if (!ennemis.Exists(e => e.EstVivant()))
                break;

            tourActuel++;
            Console.WriteLine("\nAppuyez sur une touche pour continuer...");
            Console.ReadKey();
        }

        Console.WriteLine("\n🎉 Tous les ennemis sont vaincus ! Partie terminée !");
    }

    private void AfficherEtat()
    {
        Console.WriteLine("\n--- État des ennemis ---");
        foreach (var e in ennemis)
            Console.WriteLine(e);
    }

    // Sauvegarde (optionnelle)
    public void SauvegarderEtat(string chemin)
    {
        using (StreamWriter sw = new StreamWriter(chemin))
        {
            foreach (var e in ennemis)
                sw.WriteLine($"{e.NomType},{e.Sante},{e.Vitesse},{e.Position}");
        }
    }

    // Chargement (optionnelle)
    public void ChargerEtat(string chemin)
    {
        ennemis.Clear();
        foreach (var ligne in File.ReadAllLines(chemin))
        {
            var data = ligne.Split(',');
            string type = data[0];
            double s = double.Parse(data[1]);
            double v = double.Parse(data[2]);
            double p = double.Parse(data[3]);

            Ennemi g = type switch
            {
                "Gobelin" => new Gobelin(),
                "Orque" => new Orque(),
                "Troll" => new Troll(),
                _ => null
            };

            if (g != null)
            {
                g.Sante = s;
                g.Vitesse = v;
                g.Position = p;
                ennemis.Add(g);
            }
        }
    }
}

// ==================== MAIN ====================
class Program
{
    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Jeu jeu = new Jeu();
        jeu.Jouer();
    }
}

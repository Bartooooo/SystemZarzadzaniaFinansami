#System Zarz¹dzania Finansami - wolisz DOCX -> https://docs.google.com/document/d/19J2f9DS5mNVVmVeV9drxJBHc1ja9KOMKhBK3UdeIDgU/edit?usp=sharing
Potrzebujesz instrukcji obs³ugi -> https://docs.google.com/document/d/1hiQK_jSUCSmen9Bqr4V4QsMqa9y_hK4R/edit?usp=drive_link&ouid=113703944221144580666&rtpof=true&sd=true

#### Opis Aplikacji
Aplikacja webowa umo¿liwia u¿ytkownikom œledzenie swoich finansów poprzez dodawanie dochodów i wydatków, przypisywanie ich do okreœlonych kategorii oraz generowanie miesiêcznych raportów. U¿ytkownicy mog¹ rejestrowaæ siê, logowaæ, zarz¹dzaæ swoimi finansami oraz przegl¹daæ podsumowania i wykresy. Dziêki przejrzystemu interfejsowi u¿ytkownik ma ³atwy dostêp do informacji o swoim bud¿ecie, co pozwala na efektywne zarz¹dzanie finansami osobistymi. 

**

------------------------------
## 1. Instrukcja Uruchomienia Projektu

#### Wymagania systemowe:
- System operacyjny: Windows 10 lub nowszy.
- Œrodowisko programistyczne: Visual Studio 2022 w wersji polskiej (lub nowszej).
- Framework: .NET 8.0.
- Baza danych: Microsoft SQL Server (localdb).
- Pakiety NuGet: Automatycznie pobierane podczas pierwszego uruchomienia projektu.

#### Kroki instalacji i uruchomienia:
1. **Klonowanie repozytorium:**
   git clone https://github.com/Bartooooo/SystemZarzadzaniaFinansami.git
2. **PrzejdŸ do folderu projektu:**
   cd SystemZarzadzaniaFinansami
3. **Otworzenie projektu w Visual Studio:**
    Otwórz Visual Studio 2022.
    Wybierz "Otwórz projekt lub rozwi¹zanie" i wska¿ plik .sln.
4. **Sprawdzenie konfiguracji bazy danych:**
    Otwórz plik appsettings.json.
    SprawdŸ konfiguracjê po³¹czenia z baz¹ danych.
5. **Przygotowanie bazy danych:**
    W Konsoli Mened¿era Pakietów w Visual Studio wykonaj:
      Update-Database
6. **Uruchomienie projektu:**
    Kliknij Ctrl + F5 lub przycisk "Uruchom".
    Aplikacja uruchomi siê pod adresem https://localhost:<port>.
7. **Testowanie aplikacji:**
    Zarejestruj siê i zaloguj, aby korzystaæ z funkcjonalnoœci zarz¹dzania finansami.
1. 
## 2. Struktura Projektu

#### G³ówne komponenty:

- **Kontrolery (Controllers):** Zarz¹dzaj¹ logik¹ aplikacji i przetwarzaniem ¿¹dañ.
  - Przyk³ady: HomeController, CategoriesController, IncomesController, ExpensesController, ReportsController.
- **Modele (Models):** Reprezentuj¹ dane i ich strukturê w aplikacji.
- **Widoki (Views):** Renderuj¹ dane dostarczane przez kontrolery w interfejsie u¿ytkownika.
- **Warstwa danych (Data):** Odpowiada za interakcjê z baz¹ danych za pomoc¹ ApplicationDbContext.
- **Zarz¹dzanie u¿ytkownikami (Identity):** Obs³uguje logowanie, rejestracjê i autoryzacjê.


#### Struktura katalogów:
- Controllers: Kontrolery aplikacji.
- Models: Klasy reprezentuj¹ce dane aplikacji.
- Views: Pliki HTML zintegrowane z Razor do renderowania UI.
- Data: Klasy do obs³ugi bazy danych (m.in. ApplicationDbContext).
- Areas/Identity: Pliki zarz¹dzaj¹ce uwierzytelnianiem i autoryzacj¹ u¿ytkowników.


## 3. Opis Systemu U¿ytkowników

#### Logowanie:
- Obs³uga logowania poprzez adres e-mail i has³o.
#### Walidacje:
- E-mail: Wymagane, format e-mail.
- Has³o: Wymagane, minimalna d³ugoœæ 6 znaków.
#### Rejestracja:
- Formularz rejestracyjny obejmuje:
  - Adres e-mail.
  - Has³o.
  - Potwierdzenie has³a.
- Weryfikacja adresu e-mail poprzez wysy³anie linku aktywacyjnego.
- Has³o musi spe³niaæ wymogi bezpieczeñstwa (min. 6 znaków, wielkie litery, cyfry).
#### Autoryzacja:
- Dostêp do panelu zarz¹dzania finansami jest ograniczony do zalogowanych u¿ytkowników.
#### Role u¿ytkowników:
- **U¿ytkownik:** Zarz¹dza swoimi danymi finansowymi.
#### Komponenty Identity:
- Obs³uga logowania, rejestracji i resetu has³a poprzez odpowiednie strony w Areas/Identity.
- Obs³uga zewnêtrznych metod logowania (np. Google, Facebook).
## 4. Opis Funkcjonalnoœci
#### Generowanie Raportów:
- **Opcje wyboru:**
  - Zakres dat.
  - Typ raportu: Przychody, Wydatki, Wszystko.
  - Kategoria (opcjonalna).
- **Dostêpne funkcje:**
  - Generowanie dynamicznych wykresów finansowych (s³upkowych).
  - Eksport danych do formatu CSV.
- **Walidacje:**
  - Data pocz¹tkowa nie mo¿e byæ póŸniejsza ni¿ data koñcowa.
#### Zarz¹dzanie Przychodami i Wydatkami:
- Tworzenie, edycja, usuwanie, przegl¹d.
- **Dane walidowane pod k¹tem:**
  - Kwoty (min. 0.01, max. 10 000 000).
  - Daty (wymagana).
  - Kategorii (wymagana)
#### Zarz¹dzanie Kategoriami:
- Tworzenie, edycja, usuwanie kategorii finansowych.
- Kategorie s¹ przypisane do u¿ytkownika.
- **Ograniczenia:**
  - Nazwa kategorii: Min. 1 Max. 30 znaków, unikalna dla u¿ytkownika.
## 5. Szczegó³owy Opis Modeli i Kontrolerów
#### Modele:
- **Category:**
  - Id: Unikalny identyfikator kategorii.
  - Name: Nazwa kategorii.
  - UserId: Identyfikator u¿ytkownika.
- **Income:**
  - Id: Unikalny identyfikator przychodu.
  - Amount: Kwota przychodu.
  - Date: Data przychodu.
- **Expense:**
  - Id: Unikalny identyfikator wydatku.
  - Amount: Kwota wydatku.
  - Date: Data wydatku.
#### Kontrolery:
- **HomeController:**
  - Index: Wyœwietla stronê g³ówn¹ dla zalogowanego u¿ytkownika, pokazuj¹c podsumowanie finansowe bie¿¹cego miesi¹ca.
  - Guest: Wyœwietla stronê dla niezalogowanych u¿ytkowników.
  - GenerateChart: Generuje wykres s³upkowy przychodów i wydatków u¿ytkownika za bie¿¹cy miesi¹c.
- **CategoriesController:**
  - Index: Wyœwietla listê kategorii dla zalogowanego u¿ytkownika.
  - Details: Wyœwietla szczegó³y wybranej kategorii.
  - Create: Obs³uguje tworzenie nowej kategorii.
  - Edit: Obs³uguje edycjê istniej¹cej kategorii.
  - Delete: Obs³uguje usuwanie kategorii.
- **IncomesController:**
  - Index: Wyœwietla listê przychodów u¿ytkownika.
  - Details: Wyœwietla szczegó³y wybranego przychodu.
  - Create: Obs³uguje dodawanie nowego przychodu.
  - Edit: Obs³uguje edycjê istniej¹cego przychodu.
  - Delete: Obs³uguje usuwanie przychodu.
- **ExpensesController:**
  - Index: Wyœwietla listê wydatków u¿ytkownika.
  - Details: Wyœwietla szczegó³y wybranego wydatku.
  - Create: Obs³uguje dodawanie nowego wydatku.
  - Edit: Obs³uguje edycjê istniej¹cego wydatku.
  - Delete: Obs³uguje usuwanie wydatku.
- **ReportsController:**
  - Index: Wyœwietla stronê wyboru parametrów raportu.
  - GenerateReport: Generuje raport finansowy na podstawie podanych parametrów.
  - GenerateBarChart: Generuje dynamiczny wykres s³upkowy przychodów i wydatków.
  - ExportToCSV: Eksportuje dane raportu do pliku CSV.
## 6. Uwagi Techniczne
#### Baza Danych:
- **Tabele:**
  - Categories, Incomes, Expenses.
- **Relacje:**
  - Jedna kategoria mo¿e byæ powi¹zana z wieloma przychodami i wydatkami.
#### Walidacje Danych:
- Formularze aplikacji stosuj¹ atrybuty walidacyjne, np. [Required], [StringLength], [Range].
#### Bezpieczeñstwo:
- Has³a s¹ hashowane w bazie danych.
- Weryfikacja e-mailów i reset has³a poprzez link wysy³any na e-mail.

## Technologie
Projekt wykorzystuje:

- ASP.NET Core 8.0
- Entity Framework Core
- Microsoft SQL Server (localdb)
- Razor Pages
- Bootstrap 5
- Chart.js

## Wzorzec Projektowy: MVC
Aplikacja zosta³a zaprojektowana zgodnie z wzorcem architektonicznym Model-View-Controller (MVC):

- **Model (Models):**
  - Reprezentuje dane aplikacji oraz logikê biznesow¹.
  - W projekcie modele, takie jak Category, Income, czy Expense, definiuj¹ strukturê danych oraz regu³y ich walidacji.
- **Widok (View):**
  - Odpowiada za prezentacjê danych u¿ytkownikowi.
  - W projekcie wykorzystano Razor Pages do tworzenia dynamicznego interfejsu u¿ytkownika, który renderuje dane dostarczone przez kontrolery.
- **Kontroler (Controller):**
  - Obs³uguje ¿¹dania u¿ytkownika, zarz¹dza logik¹ aplikacji oraz przekazuje dane miêdzy modelami i widokami.
  - W projekcie kontrolery, takie jak HomeController, CategoriesController, czy ReportsController, obs³uguj¹ ró¿ne funkcjonalnoœci aplikacji, takie jak zarz¹dzanie kategoriami, generowanie raportów czy przegl¹d danych finansowych.



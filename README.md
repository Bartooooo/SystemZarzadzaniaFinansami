#System Zarz�dzania Finansami - wolisz DOCX -> https://docs.google.com/document/d/19J2f9DS5mNVVmVeV9drxJBHc1ja9KOMKhBK3UdeIDgU/edit?usp=sharing
Potrzebujesz instrukcji obs�ugi -> https://docs.google.com/document/d/1hiQK_jSUCSmen9Bqr4V4QsMqa9y_hK4R/edit?usp=drive_link&ouid=113703944221144580666&rtpof=true&sd=true

#### Opis Aplikacji
Aplikacja webowa umo�liwia u�ytkownikom �ledzenie swoich finans�w poprzez dodawanie dochod�w i wydatk�w, przypisywanie ich do okre�lonych kategorii oraz generowanie miesi�cznych raport�w. U�ytkownicy mog� rejestrowa� si�, logowa�, zarz�dza� swoimi finansami oraz przegl�da� podsumowania i wykresy. Dzi�ki przejrzystemu interfejsowi u�ytkownik ma �atwy dost�p do informacji o swoim bud�ecie, co pozwala na efektywne zarz�dzanie finansami osobistymi. 

**

------------------------------
## 1. Instrukcja Uruchomienia Projektu

#### Wymagania systemowe:
- System operacyjny: Windows 10 lub nowszy.
- �rodowisko programistyczne: Visual Studio 2022 w wersji polskiej (lub nowszej).
- Framework: .NET 8.0.
- Baza danych: Microsoft SQL Server (localdb).
- Pakiety NuGet: Automatycznie pobierane podczas pierwszego uruchomienia projektu.

#### Kroki instalacji i uruchomienia:
1. **Klonowanie repozytorium:**
   git clone https://github.com/Bartooooo/SystemZarzadzaniaFinansami.git
2. **Przejd� do folderu projektu:**
   cd SystemZarzadzaniaFinansami
3. **Otworzenie projektu w Visual Studio:**
    Otw�rz Visual Studio 2022.
    Wybierz "Otw�rz projekt lub rozwi�zanie" i wska� plik .sln.
4. **Sprawdzenie konfiguracji bazy danych:**
    Otw�rz plik appsettings.json.
    Sprawd� konfiguracj� po��czenia z baz� danych.
5. **Przygotowanie bazy danych:**
    W Konsoli Mened�era Pakiet�w w Visual Studio wykonaj:
      Update-Database
6. **Uruchomienie projektu:**
    Kliknij Ctrl + F5 lub przycisk "Uruchom".
    Aplikacja uruchomi si� pod adresem https://localhost:<port>.
7. **Testowanie aplikacji:**
    Zarejestruj si� i zaloguj, aby korzysta� z funkcjonalno�ci zarz�dzania finansami.
1. 
## 2. Struktura Projektu

#### G��wne komponenty:

- **Kontrolery (Controllers):** Zarz�dzaj� logik� aplikacji i przetwarzaniem ��da�.
  - Przyk�ady: HomeController, CategoriesController, IncomesController, ExpensesController, ReportsController.
- **Modele (Models):** Reprezentuj� dane i ich struktur� w aplikacji.
- **Widoki (Views):** Renderuj� dane dostarczane przez kontrolery w interfejsie u�ytkownika.
- **Warstwa danych (Data):** Odpowiada za interakcj� z baz� danych za pomoc� ApplicationDbContext.
- **Zarz�dzanie u�ytkownikami (Identity):** Obs�uguje logowanie, rejestracj� i autoryzacj�.


#### Struktura katalog�w:
- Controllers: Kontrolery aplikacji.
- Models: Klasy reprezentuj�ce dane aplikacji.
- Views: Pliki HTML zintegrowane z Razor do renderowania UI.
- Data: Klasy do obs�ugi bazy danych (m.in. ApplicationDbContext).
- Areas/Identity: Pliki zarz�dzaj�ce uwierzytelnianiem i autoryzacj� u�ytkownik�w.


## 3. Opis Systemu U�ytkownik�w

#### Logowanie:
- Obs�uga logowania poprzez adres e-mail i has�o.
#### Walidacje:
- E-mail: Wymagane, format e-mail.
- Has�o: Wymagane, minimalna d�ugo�� 6 znak�w.
#### Rejestracja:
- Formularz rejestracyjny obejmuje:
  - Adres e-mail.
  - Has�o.
  - Potwierdzenie has�a.
- Weryfikacja adresu e-mail poprzez wysy�anie linku aktywacyjnego.
- Has�o musi spe�nia� wymogi bezpiecze�stwa (min. 6 znak�w, wielkie litery, cyfry).
#### Autoryzacja:
- Dost�p do panelu zarz�dzania finansami jest ograniczony do zalogowanych u�ytkownik�w.
#### Role u�ytkownik�w:
- **U�ytkownik:** Zarz�dza swoimi danymi finansowymi.
#### Komponenty Identity:
- Obs�uga logowania, rejestracji i resetu has�a poprzez odpowiednie strony w Areas/Identity.
- Obs�uga zewn�trznych metod logowania (np. Google, Facebook).
## 4. Opis Funkcjonalno�ci
#### Generowanie Raport�w:
- **Opcje wyboru:**
  - Zakres dat.
  - Typ raportu: Przychody, Wydatki, Wszystko.
  - Kategoria (opcjonalna).
- **Dost�pne funkcje:**
  - Generowanie dynamicznych wykres�w finansowych (s�upkowych).
  - Eksport danych do formatu CSV.
- **Walidacje:**
  - Data pocz�tkowa nie mo�e by� p�niejsza ni� data ko�cowa.
#### Zarz�dzanie Przychodami i Wydatkami:
- Tworzenie, edycja, usuwanie, przegl�d.
- **Dane walidowane pod k�tem:**
  - Kwoty (min. 0.01, max. 10 000 000).
  - Daty (wymagana).
  - Kategorii (wymagana)
#### Zarz�dzanie Kategoriami:
- Tworzenie, edycja, usuwanie kategorii finansowych.
- Kategorie s� przypisane do u�ytkownika.
- **Ograniczenia:**
  - Nazwa kategorii: Min. 1 Max. 30 znak�w, unikalna dla u�ytkownika.
## 5. Szczeg�owy Opis Modeli i Kontroler�w
#### Modele:
- **Category:**
  - Id: Unikalny identyfikator kategorii.
  - Name: Nazwa kategorii.
  - UserId: Identyfikator u�ytkownika.
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
  - Index: Wy�wietla stron� g��wn� dla zalogowanego u�ytkownika, pokazuj�c podsumowanie finansowe bie��cego miesi�ca.
  - Guest: Wy�wietla stron� dla niezalogowanych u�ytkownik�w.
  - GenerateChart: Generuje wykres s�upkowy przychod�w i wydatk�w u�ytkownika za bie��cy miesi�c.
- **CategoriesController:**
  - Index: Wy�wietla list� kategorii dla zalogowanego u�ytkownika.
  - Details: Wy�wietla szczeg�y wybranej kategorii.
  - Create: Obs�uguje tworzenie nowej kategorii.
  - Edit: Obs�uguje edycj� istniej�cej kategorii.
  - Delete: Obs�uguje usuwanie kategorii.
- **IncomesController:**
  - Index: Wy�wietla list� przychod�w u�ytkownika.
  - Details: Wy�wietla szczeg�y wybranego przychodu.
  - Create: Obs�uguje dodawanie nowego przychodu.
  - Edit: Obs�uguje edycj� istniej�cego przychodu.
  - Delete: Obs�uguje usuwanie przychodu.
- **ExpensesController:**
  - Index: Wy�wietla list� wydatk�w u�ytkownika.
  - Details: Wy�wietla szczeg�y wybranego wydatku.
  - Create: Obs�uguje dodawanie nowego wydatku.
  - Edit: Obs�uguje edycj� istniej�cego wydatku.
  - Delete: Obs�uguje usuwanie wydatku.
- **ReportsController:**
  - Index: Wy�wietla stron� wyboru parametr�w raportu.
  - GenerateReport: Generuje raport finansowy na podstawie podanych parametr�w.
  - GenerateBarChart: Generuje dynamiczny wykres s�upkowy przychod�w i wydatk�w.
  - ExportToCSV: Eksportuje dane raportu do pliku CSV.
## 6. Uwagi Techniczne
#### Baza Danych:
- **Tabele:**
  - Categories, Incomes, Expenses.
- **Relacje:**
  - Jedna kategoria mo�e by� powi�zana z wieloma przychodami i wydatkami.
#### Walidacje Danych:
- Formularze aplikacji stosuj� atrybuty walidacyjne, np. [Required], [StringLength], [Range].
#### Bezpiecze�stwo:
- Has�a s� hashowane w bazie danych.
- Weryfikacja e-mail�w i reset has�a poprzez link wysy�any na e-mail.

## Technologie
Projekt wykorzystuje:

- ASP.NET Core 8.0
- Entity Framework Core
- Microsoft SQL Server (localdb)
- Razor Pages
- Bootstrap 5
- Chart.js

## Wzorzec Projektowy: MVC
Aplikacja zosta�a zaprojektowana zgodnie z wzorcem architektonicznym Model-View-Controller (MVC):

- **Model (Models):**
  - Reprezentuje dane aplikacji oraz logik� biznesow�.
  - W projekcie modele, takie jak Category, Income, czy Expense, definiuj� struktur� danych oraz regu�y ich walidacji.
- **Widok (View):**
  - Odpowiada za prezentacj� danych u�ytkownikowi.
  - W projekcie wykorzystano Razor Pages do tworzenia dynamicznego interfejsu u�ytkownika, kt�ry renderuje dane dostarczone przez kontrolery.
- **Kontroler (Controller):**
  - Obs�uguje ��dania u�ytkownika, zarz�dza logik� aplikacji oraz przekazuje dane mi�dzy modelami i widokami.
  - W projekcie kontrolery, takie jak HomeController, CategoriesController, czy ReportsController, obs�uguj� r�ne funkcjonalno�ci aplikacji, takie jak zarz�dzanie kategoriami, generowanie raport�w czy przegl�d danych finansowych.



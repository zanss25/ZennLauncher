#### **<+ KONSEP \& DEVELOPMENT ZENN LAUNCHER +>**



&#x20; Zenn Launcher Merupakan sebuah launcher aplikasi pribadi (idealnya game) dengan sistem dimana pengguna dapat menambahkan game-game mereka ke launcher untuk dilist-kan di launcher tersebut.



**Fitur-Fitur Utama:**



1. List 'Games' dengan tiga view: Grid, Icons, dan List. Mencakup informasi: Judul, Subjudul, Deskripsi singkat, Kategori, Banner, Icon, Opsi Properties untuk edit informasi tersebut
2. Kategorisasi game sesuai kategori yang telah dibuat pengguna
3. Fitur rating untuk tiap game dan status untuk waktu terakhir launch game
4. Fitur 'Relics', yaitu fitur dimana game yang sudah pernah pengguna mainkan bisa disimpan sebagai histori game-game yang sudah pernah ia mainkan, dan akan tetap terdaftar di list game dalam kategori 'Played Before'
5. List tambahan untuk 'Tools', yaitu aplikasi non-game yang biasa digunakan untuk mengolah hal-hal yang berhubungan dengan game (seperti emulator, tools modding, dsb.)
6. Pengguna bisa menuliskan pendapatnya mengenai game tersebut di kolom 'Commentary' di tiap game yang telah ia mainkan



###### **- Elemen Utama Aplikasi: -**



**A. TABS:** Games, Tools, Categories, Relics



1. **Games**

Berisi tampilan daftar game-game pengguna dan fitur-fitur lain yang berhubungan dengan game secara menyeluruh.



**2. Tools**

Sama dengan 'Games' tapi untuk tools saja (seperti aplikasi emulator, tools modding, dsb.).



**3. Categories**

Berisi tampilan daftar game-game dan mods namun dengan menu-menu yang lebih kategoris (dipisah dengan sekat-sekat kategori).



**4. Relics**

Berisi tampilan history dan stats game yang telah dimainkan pengguna. Ada 'Relics', yaitu game-game yang sudah dihapus namun diabadikan dalam launcher ini sebagai tanda pernah memainkan game tersebut di kategori tersendiri, dengan data-datanya tetap tersimpan disana.



&#x20;============================

<+ RANDOM IDEAS FOR THE APP +>

&#x20;============================



1. A new separate app for theme creation (Themes will be stored in some kind of text files, like .json or something)



\------------------------------



**B. BASIS DATA**



**a. Global**



**1. Profiles	:** id (PK), profile\_name, hash, first\_active, data\_path



|**Nama**|**Tipe Data**|**Key**|**Default**|**Nullability**|
|-|-|-|-|-|
|id|int|Primary Key|*Set Automatically*|NOT NULL|
|profile\_name|text|—|—|NOT NULL|
|hash|text|—|—|NULL|
|first\_active|datetime|—|—|NULL|
|keep\_logged\_in|int(2)|—|0|NULL|





**2. Languages	:** id (PK), language



|**Nama**|**Tipe Data**|**Key**|**Default**|**Nullability**|
|-|-|-|-|-|
|id|int|Primary Key|*Set Automatically*|NOT NULL|
|language|text|—|—|NOT NULL|





**3. Saved Themes (\*) :** id (PK), name, color\_1, color\_2, ...



|**Nama**|**Tipe Data**|**Key**|**Default**|**Nullability**|
|-|-|-|-|-|
|id|int|Primary Key|*Set Automatically*|NOT NULL|
|name|text|—|—|NOT NULL|





**4. Global Config :** app\_version, last\_profile\_id (FK), default\_language\_id (FK)



|**Nama**|**Tipe Data**|**Key**|**Default**|**Nullability**|
|-|-|-|-|-|
|id|int(1)|Primary Key|*Set Automatically*|NOT NULL|
|config\_name|int|—|—|NOT NULL|
|value|text|—|—|NULL|
|desc|text|—|—|NULL|





**b. Per-Profile**



**1. Games	:** id (PK), title, subtitle, category\_id (FK), genre\_id (FK), user\_rating, first\_launched, last\_launched, launch\_times, target, start\_in, launch\_options, cover\_path, icon\_path



|**Nama**|**Tipe Data**|**Key**|**Default**|**Nullability**|
|-|-|-|-|-|
|id|int|Primary Key|*Set Automatically*|NOT NULL|
|title|text|—|—|NOT NULL|
|subtitle|text|—|—|NULL|
|category\_id|int|Foreign Key > categories.id|—|NULL|
|genre\_id|int|Foreign Key > genres.id|—|NULL|
|user\_rating|int(10)|—|—|NULL|
|first\_launched|datetime|—|—|NULL|
|last\_launched|datetime|—|—|NULL|
|launch\_times|int|—|0|NULL|
|target|text|—|—|NOT NULL|
|start\_in|text|—|—|NOT NULL|
|launch\_options|text|—|—|NULL|
|cover\_path|text|—|—|NULL|
|icon\_path|text|—|—|NULL|





**2. Tools	:** id (PK), title, desc, user\_rating, first\_launched, last\_launched, launch\_times, executable\_path, cover\_path, icon\_path



|**Nama**|**Tipe Data**|**Key**|**Default**|**Nullability**|
|-|-|-|-|-|
|id|int|Primary Key|*Set Automatically*|NOT NULL|
|title|text|—|—|NOT NULL|
|desc|text|—|—|NULL|
|user\_rating|int(10)|—|—|NULL|
|first\_launched|datetime|—|—|NULL|
|last\_launched|datetime|—|—|NULL|
|launch\_times|int|—|0|NULL|
|target|text|—|—|NOT NULL|
|start\_in|text|—|—|NOT NULL|
|launch\_options|text|—|—|NULL|
|cover\_path|text|—|—|NULL|
|icon\_path|text|—|—|NULL|





**3. Categories	:** id (PK), name, desc



|**Nama**|**Tipe Data**|**Key**|**Default**|**Nullability**|
|-|-|-|-|-|
|id|int|Primary Key|*Set Automatically*|NOT NULL|
|name|text|—|—|NOT NULL|
|desc|text|—|—|NULL|





**4. Genre	:** id (PK), name, desc



|**Nama**|**Tipe Data**|**Key**|**Default**|**Nullability**|
|-|-|-|-|-|
|id|int|Primary Key|*Set Automatically*|NOT NULL|
|name|text|—|—|NOT NULL|
|desc|text|—|—|NULL|





**5. Relics	:** id (PK), title, subtitle, category, genre, user\_rating, user\_commentary, first\_launched, last\_launched, launch\_times, banner\_path, icon\_path, profile\_name



|**Nama**|**Tipe Data**|**Key**|**Default**|**Nullability**|
|-|-|-|-|-|
|id|int|Primary Key|*Set Automatically*|NOT NULL|
|title|text|—|*Set Automatically*|NOT NULL|
|subtitle|text|—|*Set Automatically*|NULL|
|category|int|—|*Set Automatically*|NULL|
|genre|int|—|*Set Automatically*|NULL|
|user\_rating|int(10)|—|*Set Automatically*|NULL|
|user\_commentary|text|—|*Set Automatically*|NULL|
|first\_launched|datetime|—|*Set Automatically*|NULL|
|last\_launched|datetime|—|*Set Automatically*|NULL|
|launch\_times|int|—|*Set Automatically*|NULL|
|banner\_path|text|—|*Set Automatically*|NULL|
|icon\_path|text|—|*Set Automatically*|NULL|
|profile\_name|text|—|*Set Automatically*|NULL|

###### 

**C. SETTINGS**



**1. General**

(General App Settings)



* **Default Starting Tab		:** Choose which tab is displayed when the launcher starts (Default: Tab paling atas (Tab '0')
* **Default Games Sort By	:** Set the default sorting method (e.g., A-Z, Last Played, or Date Added) for your games
* **Default Tools Sort By	:** Set the default sorting method (e.g., A-Z, Last Launched, or Date Added) for your tools
* **Games Post-Launch Action	:** Set the launcher’s visibility once a game is running (Keep launcher open, Hide to background, Close the launcher)
* **Tools Post-Launch Action	:** Set the launcher’s visibility once a tool is running (Keep launcher open, Hide to background, Close the launcher)
* **Startup on Windows Login	:** Automatically start the launcher at Windows login

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_



**2. Profile**

(Profile Management \& Security)



* **Edit Profile			:** Edit your profile information (Change Username, Change Avatar)
* **Add Password			:** Add a password to your profile
* **Reset Password		:** Reset or change your current password
* **Backup Profile		:** Backup current profile as .zip file (data includes: statistics, settings, saves)
* **Delete Profile		:** Delete current profile

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_



**3. Appearance**

(Visuals \& UI)



* **Global Theme Color		:** Change the primary accent color of the launcher, including buttons, highlights, and borders.
* **Elemental UI Coloring	:** Individually customize colors for specific UI components like the sidebar, header, or background.
* **Component Opacity		:** Adjust the transparency levels of specific UI elements for a more layered or minimal look.
* **Typography (Fonts)		:** Choose your preferred font style and size to improve readability and match your aesthetic.
* **Scaling \& Layout Transform	:** Adjust the overall UI size (100%, 125%, 150%) to fit your screen resolution perfectly.

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_



**4. Categories**

(Content Organization)



* **Smart Auto-Categorization	:** Automatically assign new entries to specific categories based on folder source or file tags
* **Dynamic Categories		:** Create automated lists based on play behavior, such as "Recently Played" or "Most Used"
* **Category Visibility		:** Manage which categories are shown or hidden from the main navigation menu
* **Category Management		:** Create, rename, or reorder your categories in a custom list with full customization

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_



**5. Folders**

(Directories \& Storage Management)



* **Default Game Path		:** The primary folder for your games. File dialogs will point here by default (jika belum diisi atau di-reset user maka akan 				  default ke folder terakhir yang dibuka dalam dialog file sebelumnya)
* **Default Tools Path		:** The primary folder for your tools and utility apps (jika belum diisi atau di-reset user maka akan default ke folder 					  terakhir yang dibuka dalam dialog file sebelumnya)
* **Default Icons Folder		:** Preferred location for game icons (.ico, .png) (jika belum diisi atau di-reset user maka akan default ke folder terakhir 				  yang dibuka dalam dialog file sebelumnya)
* **Default Covers Folder	:** Preferred location for library cover art and posters (jika belum diisi atau di-reset user maka akan default ke folder 				  terakhir yang dibuka dalam dialog file sebelumnya)
* **Steam Library Path		:** Link your 'steamapps' folder to enable automatic Steam AppID detection
* **Screenshot Storage Folder	:** The location where in-game screenshots will be saved (Jika nanti fitur ini dikembangkan)

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_



**6. Advanced**

(For Debug, Optimization \& Error-Tracing Purposes)



* **Export Diagnostic Logs	:** Generate a detailed activity report to help identify and troubleshoot technical issues or crashes
* **Optimize Database		:** Clean up fragmented data and reduce database file size to improve performance (using VACUUM in SQLite)
* **Database Maintenance		:** Repair broken links and defragment library data to ensure stability (including Files Integrity Check)

\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_





**PENGATURAN PENAMAAN BUILD**



#### &#x20;**10230** -> Version 0.2 (Indev 3) **| 11231** -> Version 1.2.3



First Digit 	-> Prefix (To work around integers that can't start with 0)		ex. 0.2 (Indev 3) -> 1 **|** 1.2.3 -> 1

Second Digit 	-> Version Number First Digit,						ex. 0.2 (Indev 3) -> 0 **|** 1.2.3 -> 1

Third Digit	-> Version Number Second Digit, 					ex. 0.2 (Indev 3) -> 2 **|** 1.2.3 -> 2

Fourth Digit	-> Version Number Third Digit/Indev Iteration,				ex. 0.2 (Indev 3) -> 3 **|** 1.2.3 -> 3

&#x09;		-> Indev is for internal releases before initial release

&#x09;		-> Third Digit is for sub-versions after the release version

&#x09;		-> (See build's last digit)

Last Digit	-> Internal Build / Release Build					ex. 0.2 (Indev 3) -> 0 **|** 1.2.3 -> 1





###### **- Release History \& Changelog: -**



##### **A. << BETA >>**

* ###### **Version 0.1 — Zenn's First Release**



 	Rilisan perdana aplikasi Zenn Launcher tercinta. membawakan fitur-fitur dasar launcher.



 	Fitur-fitur dasar yang sudah diterapkan, diantaranya:



 	1. Games List dalam mode view 'Cover Grid'

 	2. Add New Game

 	3. Tools List dalam mode view 'Cover Grid'

 	4. Add New Tool

 	5. Edit Game/Tool Properties (Edit Info)

 	6. Delete Game/Tool

 	7. Fitur Sort by (belum lengkap)

 	8. Fitur search bar





 	Rencana perbaikan selanjutnya untuk mencapai rilisan 0.2:



 	1. Memperbaiki beberapa elemen UI yang masih 'gompel' atau kurang

 	2. Menambah support pemilihan ikon dengan file .png dan .jpg (tidak hanya .ico atau .exe)

 	3. Membuat tombol menampilkan ikon di atas teks judul ketika tidak ada gambar cover

 	4. Membuat teks pada base tombol game tidak tembus saat offside (seharusnya ke baris bawahnya)

 	5. Memberi opsi untuk membuka game tanpa menutup launcher \& membuka tool dengan menutup launcher setelahnya





* **Version 0.1.1**



   	Update ini membawa beberapa perbaikan, yaitu:



   	1. Support pemilihan ikon dengan file .png dan .jpg

   	2. Tombol game dan tools menampilkan ikon diatas teks judul ketika tidak ada gambar cover

   	3. Menambahkan tombol 'Reset' pada pemilihan cover dalam 'Game/Tool Properties'

   	4. Beberapa perbaikan UI





* **Version 0.1.2**



   	Update ini membawa beberapa perbaikan, yaitu:



   	1. Opsi untuk membuka game tanpa menutup launcher (Tekan Shift)

   	2. Beberapa perbaikan UI





* **Version 0.1.3**



   	Update ini membawa beberapa perbaikan, yaitu:



   	1. Memperbaiki masalah dimana launcher akan crash saat izin program yang di-launch tidak diberikan (Ketika mengklik 'No' atau 'Cancel') dan error 	handler umum lainnya

   	2. Memperbaiki masalah dimana ikon tidak tampil di tombol game ketika ikon berasal dari file .exe atau .lnk

   	3. Mengganti direktori data launcher ke folder 'Documents'

   	4. Beberapa perbaikan UI





* ###### **Version 0.2 — More Freedom in Your Own Room**



   	Major update ini berfokus pada menghidupkan fitur 'Profiles' dalam launcher, memungkinkan daftar dan konfigurasi yang berbeda untuk setiap profile-nya. Selain itu juga diperkenalkan 'Settings' untuk personalisasi pengguna yang lebih fleksibel, juga upgrade yang cukup signifikan pada UI aplikasi.

  &#x09;

   	Fitur-Fitur dan perbaikan dalam update ini diantaranya:

  &#x09;

  &#x09;1. Merealisasikan fitur 'Profile' yang sudah direncanakan sebelumnya

  &#x09;2. Menambah fitur 'Settings' untuk personalisasi pengguna yang lebih fleksibel

  &#x09;3. Menambah beberapa fitur Quality of Life seperti 'Open File Location' dan beberapa lainnya

  &#x09;4. Beberapa upgrade dan perbaikan UI yang cukup signifikan (Termasuk kemungkinan penerapan font baru dan ikon)





   	Rencana perbaikan selanjutnya untuk mencapai rilisan 0.3:



   	1.





* **Version 0.2.1**



   	Update ini membawa beberapa perbaikan, yaitu:



   	1.





* ###### **Version 0.3 — Just Sort Your Games**



   	Major update ini berfokus pada membawa fitur utama yaitu fitur 'Category' untuk membantu mengatur dan mengorganisir game-game penguna.



* ###### **Version 0.4 — The Relics of the Past**



   	Major update ini berfokus pada membawakan fitur 'Relics' dan sekalian juga 'History' sebagai sarana mengingat kenangan pengguna dalam bermain game yang dulu pernah ia mainkan.



* ###### **Version 0.5 — A Fresh Coat of Paint**



   	Major update ini berfokus pada memoles UI launcher dan sentuhan-sentuhan terakhir sebelum menuju rilisan "1.0".



   


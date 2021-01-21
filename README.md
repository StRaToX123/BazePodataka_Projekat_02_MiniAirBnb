# BazePodataka_Projekat_02_MiniAirBnb
<br />
Studenti :<br />
16178 Pavle Marinković<br />
16753 Ugljesa Mitrović<br />
16628 Andjela Jovanović<br />
16597 Djordje Ivanović<br />
<br />
Cassandra Mini AirBnb<br />
Svaki korisnik ima svoj nalog, i može postavljati oglase za stanove.<br />
Svaki oglas sadrži datume dostupnosti, adresu i sliku stana.<br />
Korisnici mogu pretraživati stanove po datumimu i adresi, mogu slati zahteve za iznajmljivanje na neki odredjeni datum,<br />
kao i obrađivati zahteve za svoje stanove.<br />
<br />
<br />
Pokretanje projekta:<br />
1.<br />
Treba napraviti keyspace "MiniAirBnb"<br />
2.<br />
Popuniti keyspace sa tabelama koristeći CQL skirptu sa repozitorijuma "CQL_Script.txt"<br />
3.<br />
Otvoriti visual studio solution fajl<br />
4.<br />
Pokrenuti "Server" projekat (Ako je prazna databaza, server će je popuniti sa sample informacijama)<br />
5.<br />
Pokrenuti koliko god "Client" projekata<br />
Od sample vrednosti koje server napravi postoje tri naloga (username: pavle, password: pavle) (username:djordje, password:djordje)<br />
(username: misa, password: misa)<br />
pavle ima dva oglasa a misa jedan, i misa i djordje imaju zahtev ka pavlovom stanu. Možete isprobati odobravanje ili odbijanje zahteva, dodavanje novih zahteva, <br />
dodavanje ili brisanje stanova, kao i pretraga postojećih stanova<br />
<br />
<br />
Dodatne informacije:<br />
1.<br />
Prenos slika između servera i klijenta se odvija veoma sporo kod velikih slika. Kod prenosa dosta velikih slika može izgledati kao da se ništa ne dešava, ali polako<br />
prebaciće se slika<br />
2.<br />
Oglasi su prikazani kroz dataGridView, možete koristiti horizontalni i vertikalni slider dataGridView-a kako bi pogledali i ostale informacije oglasa, pogotovo datume kod<br />
obradjivanja zahteva<br />
3. !!!!!! VEOMA BITNO !!!!!!<br />
Duplim klikom na oglas koji dobijemo kao rezultat pretrage se otvara forma za postavljanje zahteva<br />
4.<br />
Pri ubacivanju novog stana, program će zahtevati da se datumi dostupnosti unove po rastućem redosledu<br />
5. !!!!!! VEOMA BITNO !!!!!!<br />
Klikom na dugme "Moji Oglasi" se otvara nova forma sa dataGridView-om oglasa koje je postavio korisnik. Klikom na neke od ovih oglasa će se sa desne strane prikazati<br />
zahtevi koje taj oglas ima. Klikom na neki zahtev a zatim na dugme "Odobri" ili "Odbij" se menjaju datumi dostupnosti stana (možda je potrebno horizontalno skrolovati u<br />
dataGridView-u kako bi se videla kolona datumi). Neće biti moguće prihvatiti sve zahteve, zato je tu "Undo" dugme kako bi mogli da se vratimo jedan korak u nazad pre nego<br />
što smo prihvatili ili odbili neki zahtev (Uočiti promene u koloni datumi, oglasa čije zahteve obrađujemo). Kada korisnik bude bio zadovoljan sa obradom zahteva može kliknuti<br />
dugme "Potvrdi Izmene" kako bi konačno stanje prosledio serveru za upis u data bazu<br />
<br />
<br />
<img src="https://i.imgur.com/CnxDNPF.jpg"/>

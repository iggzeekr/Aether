# AETHER

Unity ile sıfırdan kurulmuş, oynanabilir bir bilim-kurgu şehir oyunu. Amacım bir mağaza oyunu çıkarmak değil. Amacım oyun tasarımını, sahne kurmayı, kamerayı, görev döngüsünü öğrenmek.

AETHER’in çekirdeği sönmek üzere. Asuna, şehrin içinde kalan son bilim insanı. Beş dakikası, üç canı ve gemisi var.
Bitirmesi gereken üç iş var: laboratuvardaki sekiz görev, sokaktaki on sinyal kulesi, gökyüzündeki halkalar. Üçünü de süre bitmeden tamamlarsa görev biter. Süre biterse oyun kilitlenir. Üç kez ölürse **Game over**.Aynı zamanda uzay gemileriyle çarpışırsa canı azalır
İki dakika sonra gece çöker. Sokak lambasını yakamazsa karanlıkta kalır.
---

## Bu projede ne öğrenmek istedim??

Küçük parçaları ayrı ayrı değil, tek bir oyunun içinde bir arada tutmak:

- Oyuncu ne yapıyor, neden tekrar yapmak istiyor.
- Hazır model ile kendi kodunun birbirine uyması. Model duruyor diye oyun durmaz. Materyal, kemik, pivot ve collider ayrı problemler.

---

## Nasıl oynanır

Unity’de projeyi aç. Sahne boş görünür. **Play**’e bas. Başlangıç ekranı gelir. **Enter** veya sol tık ile başla.

| Tuş | Ne yapar |
| --- | --- |
| WASD | Yürü |
| Shift | Koş |
| Space | Zıpla. Gemideyken yüksel |
| Ctrl veya C | Gemide alçal |
| F | Gemiye bin. Yükseklik 8 metrenin altındayken in |
| E | Laboratuvar işi, sokaktaki sinyal kulesi, kapı |
| Sol tık | Ateş. Artı kırmızıysa yakındaki robota kilitlenir |
| Sağ tık basılı | Kamerayı çevir |
| Turkuaz kapı | Laboratuvara gir |

Laboratuvarın sekiz işi var. Süre **60 saniye**. Bitirmezsen dışarı atılırsın, işler sıfırlanır. Çizelge yalnız içeride görünür.

Sokakta on sinyal kulesi var. Gökyüzünde halkalar var. Gemiyle halkanın içinden geç. Bunların sayacı şehirde, altta.

Can sol üstte. Robot yakında ve ekrandaysa tek atış onu devirir. Bir süre sonra kalkar.

---

## Oyun döngüsü

Üç kısa döngü var. Hepsi aynı karakterle, aynı şehirde.

1. **Yürü ve tara.** Kapıya git, kuleye E bas, halkayı gör.
2. **Uç.** Gemiye bin, yüksel, halkadan geç, alçal, in. İnmek için önce yere yaklaşmak gerekiyor. Gökyüzünde F işe yaramaz.
3. **Laboratuvar.** İçeri gir, numaralı işleri sırayla bitir, süre bitmeden çık.

Çatışma dördüncü döngü. Robotlar sürekli saldırmaz. Yaklaşınca durur, nişan alır, turuncu ışık atar. Senin ışığın mavi.

---

## Kodun haritası

Hepsi `Assets/Scripts` altında. Ayrı bir sahne yöneticisi yok. `CityGame` omurga.

| Dosya | Görevi |
| --- | --- |
| `CityLayout` | Izgara ölçüleri |
| `CityBuilder` | Şehir, asker, gemi, halka, gökyüzü |
| `CityArt` | Modelleri editörde yükler |
| `PlayerMotor` | Yürü, koş, zıpla |
| `CharacterVisual` | Asuna modeli ve tabanca soketi |
| `CityCamera` | Üçüncü şahıs. Gemide birinci şahıs |
| `DriveableCar` | Geminin uçuş fiziği |
| `LabDoor` / `LabInterior` | Kapı, oda, sekiz iş, süre |
| `FieldScan` | Sokaktaki on kule |
| `SkyRing` / `SkyLane` | Halka ve dönen gemiler |
| `GameMenu` | Başlangıç ekranı. Bitene kadar zaman durur |
| `PlayerCombat` / `EnergyBolt` / `Vital` | Ateş, can, kilit |
| `SoldierFight` / `SoldierMarch` | Robotun tüfeği, saldırısı, yürüyüşü |
| `GunMount` | Silahı kemiğe oturtur |

`DeliveryMission` duruyor ama oyuna bağlı değil. Eski bir teslimat denemesi.

---

## Karşılaştığım teknik sorunlar

### 1. Modeller pembeydi

Belirti: Asuna ve gemiler düz pembe.

Neden: Proje Built-in. Birçok Asset Store paketi URP Lit shader’ı kullanıyor. O shader bu projede yok. Unity, shader’ı bulamayınca magenta basıyor. Saç materyali de eksik slota bakıyordu.

Ne yaptım: Gemilerin materyallerini `_BaseMap` okuyan kendi Built-in shader’ıma çevirdim. Karakter için ayrı bir shader. Saç slotunu paketin içindeki kahverengi saça bağladım. Projeyi URP’ye taşımadım. Bir paketi kurtarmak için bütün ışık ve materyal düzenini değiştirmek daha büyük bir iş.

Ders: Paketi indirmeden önce pipeline’ına bak. Pembe, “model bozuk” demek değil. “Shader bu projede yok” demek.

### 2. Laboratuvar boştu, karakter gökyüzündeydi

Belirti: İçeri girince mavi boşluk. Sonra sekizgen bir koridor, kamera dışarıda.

Neden: Sci-fi paket 4 metrelik ızgarada. `Corridor_I` prefabının pivotu odanın ortası değil. Zemin çocuğu lokal x = 4’te, pencereler z = ±2’de, tavan y = 4’te. Prefabı “oda” diye koyunca oyuncu ve kamera mesh’in dışında kaldı. Kameranın arka plan rengi gökyüzü mavisiydi `(0.42, 0.62, 0.84)`. Delik olan her yer gökyüzü gibi göründü. Zeminin renderer’ı kapalıysa aynı şey oluyor: collider var, görüntü yok.

Ne yaptım: Koridor prefabını oda olmaktan çıkardım. 6 x 4 karo, 24 x 16 metre, yükseklik 4. Zemin, duvar, ters çevrilmiş tavan karoları, tavan ışığı, konsol, bitki, jeneratör. Oda şehrin çok dışında. İçeride kamera arka planı koyu. İçeri girince oda yeniden kuruluyor, oyuncu belirlenen noktaya ışınlanıyor.

Ders: Prefabın adı oda diye pivotu ortada olmak zorunda değil. Kameranın boşlukta gördüğü renk, “dünya yok” ile “dünya var ama ben dışarıdayım”ı ayırır.

### 3. Halkalar beyaz kutuydu

Belirti: Gökyüzünde küp dizisi ve yerden göğe bir ışık direği.

Neden: Halkayı küp parçalarıyla kurmuştum. Emission rengi `renk * 2.4` olunca Standard shader beyaza patlıyor. Direk de “buradayım” diye eklenmiş bir küptü. Okunuyordu, güzel durmuyordu.

Ne yaptım: İki ince silindir çember. Dış yarıçap 11, iç 9.2. Emission daha kısık. Direk yok, numara yok. Gemi tetiğe girince çember yeşile dönüyor.

Ders: Parlaklık dikkat çeker, biçimi öldürür. Silüet önce gelsin.

### 4. Gemi yukarı çıkmıyordu

Belirti: Space basıyorum, gemi yerde sürünüyor. Ya da dönüyor ama eğilmiyor.

Neden: Rigidbody’de `FreezePositionY` vardı. Yüksekliği kod değil, kısıt kilitliyordu. Eğim için `MoveRotation` kullanırken X/Z dönüş kısıtı da duruyordu. Kısıt, o karede yazdığın dönüşü eziyor.

Ne yaptım: Uçuşta yerçekimi yok, pozisyon ve dönüş kısıtı yok. İleri hız, yükselme ve yatış script’te. Yükseklik 3.2 ile 90 metre arasında. Şehir sınırının dışına çıkmıyor. İnmek Ctrl veya C, basılı tutarak. F yalnız alçakta.

Ders: Fizik bileşeni ile hareket kodu aynı eksene sahip çıkmasın. Biri yönetsin.

### 5. Gemiye binince hâlâ dışarıdan bakıyordum

Belirti: Kokpitte olmak istiyorum, kovalamaca kamerası geminin kabuğunu gösteriyor.

Neden: Üçüncü şahıs kamera hedefe mesafeli duruyor. Gemi mesh’i kamerayla koltuk arasında.

Ne yaptım: Binince geminin renderer’ları kapanıyor, kamera koltuk noktasına geçiyor, oyuncu modeli gizleniyor. İnince ikisi de geri geliyor. Kamera duvara yapışmasın diye ışın, oyuncunun layer’ını (layer 2) görmezden geliyor. Aksi halde ışın kendi bedenine çarpıp kamerayı içeri çekiyor.

Ders: “Ben oldum” hissi, kamerayı yaklaştırmak değil. Gövdeyi görmemek.

### 6. Asker yürümüyordu

Belirti: Cyber soldier ayakta kayıyor.

Neden: FBX humanoid. Animasyon tipi doğru. İçinde yürüme klibi yok. `clipAnimations` boş. Animator’a “Walk” demek, klip yoksa hiçbir şey yapmıyor.

Ne yaptım: `SoldierMarch`, `LateUpdate` içinde kalça, diz ve kol kemiklerini sinüs ile sallıyor. Animasyon değil. Prosedürel yürüyüş. Görünür olması için yeterli.

Ders: İçe aktarılan karakter, içe aktarılan animasyon demek değil. Animation sekmesine bak.

### 7. Yanımdaki robotu öldüremiyordum

Belirti: Robot dibimde. Sol tık. Hiçbir şey olmuyor.

Neden, üç parçalı:

- Üçüncü şahıs kamera oyuncuya bakıyor. Ekranın ortası robot değil, kızın kafası. Işın kameradan çıkıp onun içinden geçip sokağa gidiyor.
- Askerin çarpışması trigger. Yürürken birbirine takılmasınlar diye. Varsayılan raycast trigger’ı görmez. Mermi fizikle gidince yere çarpıp yok oluyor, bedene değmiyor.
- Tabanca soketi elde. Nişan soketin baktığı yer değil, kameranın baktığı yer. İkisi yan yana durunca ayrılıyor.

Ne yaptım: Atış artık saf fizik değil. Ekranda görünen ve yakındaki canlı asker seçiliyor. Canı anında iniyor. Mavi ışık sadece görüntü. Artı, kilit varsa kırmızı. Tek atış deviriyor. Düşünce model yana yatıyor, sonra kalkıyor.

Ders: Üçüncü şahısta “kameranın ileri yönü” ile “dibimde duran düşman” aynı şey değil. Vuruşu görsel mermiden ayır. Trigger, yürüyüş için iyi, isabet için kör.

### 8. Tüfek elde durmuyordu

Belirti: Robotun tüfeği bel hizasında, yana yatık, elde değil.

Neden: Asuna’da `Firearm_SocketPistol_R` diye bir soket kemiği var. Tabancayı oraya, lokal sıfır dönüşle koyunca paket onu ele göre ayarlamış oluyor. Askerde soket yok. Sadece `Hand.R` var. O kemiğin “ileri” ekseni karakterin ileri yönü değil. Tüfeğin uzun ekseni de prefabda her zaman Z değil. İskelet ölçeklenince 1 birimlik silah dev ya da toplu iğne oluyor.

Ne yaptım: Tabanca sokete bağlı. Tüfekte mesh’in en uzun eksenini bulup gövdenin ileri yönüne çeviriyorum, dünya boyunu yaklaşık 0.78 metreye çekiyorum, kabzayı ele yaslıyorum.

Ders: “Instantiate et, ele parent yap” silah tutmak değil. Namlu hangi eksen, el hangi eksen, ölçek kimin, üçüne ayrı bak.

### 9. Laboratuvar sayacı kendi kendine bozulabiliyordu

Belirti: Oda yeniden kurulunca işler sıfır görünüyor ama “3/8 bitti” yazısı kalıyor. Ya da süre, şehre daha hiç girmemişken akıp bitiyor.

Neden: Odayı kuran fonksiyon ile “oyuncu içeri girdi” aynı an değil. Şehir açılırken oda bir kez kuruluyor. Süre orada başlarsa oyuncu sokaktayken dolar. Bitmiş iş sayacı ise eski ziyaretten kalıyor, yeni noktalar `Done = false` doğuyor.

Ne yaptım: Süre yalnız `Enter` içinde başlıyor. Oda kurulunca tamamlanan sayı sıfırlanıyor. Süre bitince işler silinip kapıya atılıyor. Ölünce laboratuvar “başarı” sayılmıyor.

Ders: Geometriyi sıfırladığın kare, oyun durumunu da sıfırlamalı. Açılış ile “bölüme girmek” ayrı olay.

### 10. Modeller sadece editörde yükleniyor

Belirti: Bu bir oynanış hatası değil. Bir sonraki duvar.

Neden: `CityArt`, `AssetDatabase.LoadAssetAtPath` kullanıyor. Bu API yalnız editörde var. `#if UNITY_EDITOR` ile sarılı. Build alırsan yollar boş döner, şehir modelleriz kalır.

Ne yaptım: Şimdilik oyun Unity editöründe, Play ile oynanıyor. Bilerek böyle. Öğrenme projesi.

Ders: Editör kısayolu, oyuncu build’i değil. Dışarı çıkacak sürüm için modellerin `Resources`, Addressables ya da sahneye konmuş referans olması gerekir.

---

## Repoda olmayanlar

Bu repoda kod, shader ve Unity proje ayarları var. Modeller yok. Asset Store paketlerini indirip aynı klasör adlarıyla `Assets` altına koyman gerekir. Kod yolları bu isimlere bakıyor:

- `Assets/FreeTestCharacterAsuna`
- `Assets/HiRezSpaceshipsCreatorFree`
- `Assets/Sci-Fi Styled Modular Pack`
- `Assets/MyAssets/CyberSoldier`

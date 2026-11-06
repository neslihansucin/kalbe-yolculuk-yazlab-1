# Kalbe Yolculuk

**Yazılım Geliştirme Laboratuvarı-I · Proje 1 — Yapay Zekâ Destekli Macera Oyunu**  
**Kocaeli Üniversitesi · Teknoloji Fakültesi · Bilişim Sistemleri Mühendisliği**

| **Proje bilgisi**           | **Değer**                                                                  |
| --------------------------- | -------------------------------------------------------------------------- |
| Proje ekibi                 | **Feride Güner** (ferideguner89-hash) — **Neslihan Suçin** (neslihansucin) |
| Akademik yıl / rapor tarihi | 2026–2027 / Kasım 2026                                                     |
| Oyun motoru                 | Unity 6.6 (6000.6.3f1)                                                     |
| Geliştirme dili / editör    | C# / Visual Studio Community 2022 (17.14.41, 64-bit)                       |
| Otonom karakter             | NavMesh + Raycast + sonlu durum makinesi (FSM) ile hareket eden kedi       |
| Yerel LLM                   | Ollama üzerinde gemma3:4b — Kalp Muhafızı NPC                              |

## 1. Giriş

### 1.1 Problemin Tanımı ve Amaç

**Problem:** Oyuncunun tek başına aşamadığı fiziksel engellerin otonom yapay zekâ yardımıyla çözülmesi ve oyun içi sosyal/mantıksal etkileşimlerin sabit diyaloglar yerine dinamik hâle getirilmesidir. Kalbin damarlarında ilerleyen 3D karakter hem fiziksel hem de mantıksal engellerle karşılaşır. Proje, Kocaeli Üniversitesi Yazılım Geliştirme Laboratuvarı-I kapsamında Unity/C# ile geliştirilmiştir.

**Amaç:** NavMesh ve Raycast kullanan bir otonom companion ile Ollama üzerinden çalışan yerel LLM destekli bir NPC'yi tek oyun içinde birleştirmek; oyuncunun kararlarının oyun durumunu etkilediği bir deneyim sunmak; bunu OOP/SOLID, asenkron iletişim ve hata yönetimiyle güvenilir biçimde gerçekleştirmektir.

Proje, tek bir Unity macera oyununda iki bağımsız yapay zekâ bileşenini birleştirir:

1.  **Otonom companion (kedi):** NavMesh üzerinde yürür, oyuncunun komutuyla duvardaki geçidi kendi sensörleriyle bulur, geçer, butonu arar ve butona basar.

2.  **Yerel LLM destekli NPC (Kalp Muhafızı):** Oyuncuyla serbest metinle konuşur. Cevabını Ollama'daki yerel modelden alır, model JSON şemasına bağlı çıktı üretir ve bu çıktı kapının açılıp açılmayacağını belirler.

Bu iki bileşen ayrı sınıflarda ve ayrı klasörlerde (Cat/, Guard/) yazılmıştır, birbirine doğrudan bağımlı değildir.

### 1.2 Oyun Senaryosu

Oyun, kalbin damarlarının içinde ilerleyen bir insan karakter ile ona eşlik eden yapay zekâ destekli kedinin macerasını anlatır. Oyuncu başlangıç menüsünde kontrol bilgilerini görür; ardından kendi başına aşamadığı engelli bir duvarla karşılaşır. Oyuncunun E komutuyla kedi duvardaki uygun deliği Raycast ile algılayıp NavMesh üzerinde diğer tarafa geçer, butonu bulur ve duvarı parçalayarak oyuncunun yolunu açar.

Engel geçildikten sonra **iki farklı yol** vardır: yollardan birinde düşman, diğerinde uzun bir geçit ve sonunda çukur bulunur. Düşmana yakalanmak veya çukura düşmek, oyuncu ile kedinin başlangıç noktasına dönmesine neden olur. Oyuncu tehlikeleri aşarak **Mutlu Anılar Küresi'ni (kalp küresini)** alır ve kapıyı koruyan **Kalp Muhafızı** ile serbest metin üzerinden konuşur. Muhafız yalnızca iyileştirme niyeti doğrulandığında **ve** küre taşınıyorsa kapıyı açar. Oyuncu açılan kapıdan kalbin merkezine gider, küreyi yerine yerleştirir; kalp merkezindeki ışıklar yanar ve bitiş ekranı gösterilir.

Oyun akışı:

```mermaid
flowchart TD
A[Ana Menü: Başla] --> B[Bilgilendirme paneli]
B --> C[MainScene yüklenir, NavMesh runtime'da oluşturulur]
C --> D[Oyuncu ilerler, kedi oyuncuyu takip eder]
D --> E[Oyuncu E tuşuyla kediye komut verir]
E --> F[Kedi duvardaki geçidi bulur ve geçer]
F --> G[Kedi butonu arar ve butona basar]
G --> H[Duvar parçalanır, koridor açılır]
H --> H2[İki yol: düşmanlı yol veya uzun geçit ve çukur]
H2 --> H3{Düşmana yakalandı mı veya çukura düştü mü?}
H3 -- Evet --> D
H3 -- Hayır --> I[Oyuncu Mutlu Anılar Küresini F ile alır]
I --> J[Oyuncu Kalp Muhafızı ile konuşur]
J --> K{Muhafız kararı}
K -- allow_entry --> L[Kapı açılır]
K -- request_more_info / deny --> J
L --> M[Oyuncu küreyi kalbin merkezine Y ile yerleştirir]
M --> N[Işıklar yanar, 2 sn sonra oyun sonu paneli]
```

### 1.3 Kontroller

| **Tuş**       | **İşlev**                                                           |
| ------------- | ------------------------------------------------------------------- |
| W / S         | İleri / geri                                                        |
| A / D ve fare | Dönüş                                                               |
| Ctrl          | Eğilme                                                              |
| E             | Kediye komut verme (CatStateMachine), etkileşim (PlayerInteraction) |
| F             | Küreyi alma                                                         |
| Y             | Küreyi kalbin merkezine yerleştirme                                 |
| F6            | Diyalog açıkken karakter ve kamera hareketini serbest bırakma       |
| ESC           | Oyun menüsü                                                         |

### 1.4 Kurulum ve Çalıştırma

#### Yerel LLM hazırlığı

Kalp Muhafızı ile konuşabilmek için [Ollama](https://ollama.com/) kurulu ve çalışıyor olmalıdır.

Modeli indirin:

```bash
ollama pull gemma3:4b
```

Ollama masaüstü uygulaması açıksa sunucu zaten arka planda çalışır. Çalışmıyorsa (veya komut satırından kullanıyorsanız) sunucuyu başlatın:

```bash
ollama serve
```

Oyun, modele `http://localhost:11434/api/generate` adresinden bağlanır.

#### Hazır Windows sürümünü çalıştırma

[Kalbe Yolculuk — Windows sürümünü indir](https://github.com/neslihansucin/kalbe-yolculuk-yazlab-1/releases/download/v1.0.0/KalbeYolculuk-Windows.zip)

1. Yukarıdaki bağlantıdan ZIP dosyasını indirin.
2. ZIP dosyasının tamamını bir klasöre çıkartın.
3. Çıkartılan klasördeki `kalbe-yolculuk-yazlab-1.exe` dosyasını çalıştırın.

> Muhafızla ilk konuşmada modelin belleğe yüklenmesi birkaç saniye sürebilir. Yanıt gelmezse biraz bekleyip mesajı tekrar gönderin.

Hazır sürümü oynamak için Unity kurulumu gerekmez.

#### Unity üzerinden çalıştırma

1. Projenin kaynak dosyalarını indirin.
2. Projeyi Unity 6 (6000.6.3f1) ile açın.
3. `MainMenu` sahnesini açıp Play düğmesine basın.

### 1.5 Ana menü ekran görüntüsü

![](docs/images/ana-ekran.png)

## 2. Otonom AI Kahraman (Kedi)

Kedi, Unity **NavMeshAgent** kullanır. Kodu Assets/Scripts/Cat/ altındadır. Karar mantığı CatStateMachine içinde bir **durum makinesi** olarak tasarlanmıştır (CatState enum'u). Hareket hedefleri sabit koordinat değil, çalışma anında sensörlerle bulunur.

### 2.1 Sensörler (Raycast)

| **Sınıf**                         | **Ne yapar**                                                                                                                                                                                                                                                                                           | **Parametreler**                                        |
| --------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ | ------------------------------------------------------- |
| CatWallScanner.TryDetectWall      | Kedinin önünde yelpaze şeklinde ışın atar, wallLayer üzerindeki ve ObstacleWallMarker taşıyan en yakın duvarı seçer                                                                                                                                                                                    | 25 ışın, 120° açı, 12 birim mesafe                      |
| CatWallScanner.TryFindGap         | Duvarın genişliği boyunca 81 ışın (80 aralık) atar. Işının engele çarpmadığı aralıkları delik olarak işaretler. Her deliğin iki yanındaki noktayı NavMesh.SamplePosition ile doğrular, NavMesh.CalculatePath ile iki tarafın bağlı olduğunu (PathComplete) kontrol eder. En yakın geçerli deliği seçer | ışın uzunluğu 2, giriş/çıkış deliğin ±0.6 birim yanında |
| CatSensor.TryFindButton           | Çevresine 360° ışın atar, Button tag'li collider'ı arar                                                                                                                                                                                                                                                | 36 ışın, 10 birim                                       |
| CatSensor.TryFindExplorationPoint | Buton görünmüyorsa çevreyi tarar, en çok boş mesafesi olan yöne yeni bir keşif noktası seçer. Daha önce ziyaret edilen noktaları (yarıçap 1.5) ve arama alanı dışındakileri eler                                                                                                                       | 36 ışın, 4 birim, adım 0.5-1.5                          |

CatWallScanner ve CatSensor MonoBehaviour değildir, CatStateMachine tarafından oluşturulup kullanılır.

### 2.2 Durum Makinesi

```mermaid
stateDiagram-v2
[*] --> FollowingPlayer
FollowingPlayer --> CrossingObstacle: E tuşu (görev tamamlanmadıysa)
CrossingObstacle --> FollowingPlayer: duvar veya geçerli delik bulunamadı
CrossingObstacle --> Searching: delikten karşıya geçildi
Searching --> MovingToButton: sensör butonu gördü
Searching --> FollowingPlayer: yeni keşif noktası kalmadı
MovingToButton --> ActivatingButton: butona 0.5 birimden yakın
MovingToButton --> Searching: buton bilgisi kayboldu
ActivatingButton --> TaskComplete
TaskComplete --> FollowingPlayer: 2.5 sn sonra
```

Butona basma etkisi CatButtonTrigger içindedir: Cat tag'li obje buton trigger'ına girince BreakableWall.BreakWall() çalışır (collider'lar kapanır, parçalar savrulur, parçacık efekti oynar) ve corridorWall kapatılır.

**Şekil 1 — Otonom kedinin FSM durum diyagramı:**

![Görsel 1](docs/images/diyagram1.png)

Bu diyagram oyuncuyu takip etme, engeli aşma, butonu arama ve görevi tamamlama durumları ile geçişlerini gösterir.

### 2.3 Karar Akışı

```mermaid
flowchart TD
A[E tuşu: CrossingObstacle] --> B{obstacleWall atanmış mı?}
B -- Hayır --> C[TryDetectWall: 25 ışınlı yelpaze]
C --> D{Duvar bulundu mu?}
D -- Hayır --> R[ReturnToPlayer]
B -- Evet --> E
D -- Evet --> E[TryFindGap: 81 ışınla delikleri çıkar]
E --> F{NavMesh ile bağlı geçerli delik var mı?}
F -- Hayır --> R
F -- Evet --> G[En yakın deliği seç]
G --> H[Giriş noktasına git]
H --> I[Çıkış noktasına git]
I --> J[Searching]
J --> K{Sensör butonu gördü mü?}
K -- Evet --> L[MovingToButton]
K -- Hayır --> M{Yeni keşif noktası var mı?}
M -- Evet --> J
M -- Hayır --> R
L --> N[Butona ulaştı]
N --> O[TaskComplete]
```

Şekil 2 — Otonom kedinin davranış akış diyagramı:

![Görsel 2](docs/images/diyagram2.png)

Diyagram, Raycast ile geçidi belirleme, NavMesh üzerinden geçme, yeni bölgeleri keşfetme ve butonla duvarı açma sürecini gösterir.

### 2.4 Çalışma Zamanı NavMesh

MainSceneNavMeshInitializer, MainScene açılırken NavMeshSurface.BuildNavMesh() ile NavMesh'i runtime'da üretir. Duvar kırıldığında BreakableWall, NavMeshSurface.UpdateNavMesh ile NavMesh'i yeniden günceller, böylece açılan yol kedi için de yürünebilir olur.

SphereText ve HeartRepairText adlı 3D TextMeshPro objeleri geçersiz mesh verisi ürettiği için, NavMesh build kaynaklarından NavMeshModifier (Ignore From Build) ile çıkarılmıştır.

### 2.5 Diğer NavMesh Kullanan Bileşenler

- **Düşman (Enemy/):** EnemyPatrol devriye noktaları arasında NavMesh ile gezer. EnemyDetection görüş açısı ve Raycast ile oyuncuyu algılar, EnemyChase kovalar. EnemyCatch yakalayınca oyuncuyu ve kediyi başlangıç noktasına gönderir. Oyuncu küreyi almışsa yakalanmaz.

- **Oyuncu hareketi:** PlayerController, hareketi NavMesh üzerinde sınırlandırır (NavMesh.SamplePosition, NavMesh.Raycast).

## 3. LLM Entegrasyonu (Kalp Muhafızı)

Kod Assets/Scripts/Guard/ altındadır.

### 3.1 Model ve Bağlantı

| **Alan**        | **Değer (kodda)**                                                                                                                                                                                                                                                                                                         |
| --------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Model           | gemma3:4b                                                                                                                                                                                                                                                                                                                 |
| Adres           | http://localhost:11434/api/generate                                                                                                                                                                                                                                                                                       |
| İstek tipi      | stream: false, format alanında JSON şeması                                                                                                                                                                                                                                                                                |
| Zaman aşımı     | 15 saniye (HttpClient.Timeout)                                                                                                                                                                                                                                                                                            |
| Seçim gerekçesi | Geliştirme sürecinde birkaç farklı yerel model test edilmiştir. Diğer modellerin Türkçe yanıtlarında zaman zaman anlatım bozuklukları ve konu dışına çıkmalar görülmüştür. gemma3:4b ise daha akıcı ve anlaşılır Türkçe cümleler kurduğu, ayrıca oyun içi diyaloglara daha doğal yanıtlar verdiği için tercih edilmiştir. |

### 3.2 System Prompt

NPC'nin kişiliği ve görevi GuardPromptBuilder.BasePrompt içinde tanımlıdır. Oyun durumu (hasMemoryOrb, hintLevel) ve oyuncu mesajı bu metnin sonuna eklenip Ollama'ya **tek bir prompt alanı** olarak gönderilir.

```text
Sen Kalp Muhafızısın.
Oyuncunun mesajını değerlendir ve intent, action ve reply üret.
Muhafız kişiliği sadece reply üslubunu etkiler. Action kararını değiştiremez.
INTENT:
- heal: oyuncu kalbi iyileştirmek, canlandırmak veya korumak istiyor.
- harmful: oyuncu kalbe, merkeze veya anılara zarar vermek, yok etmek, silmek veya parçalamak istiyor.
- unclear: oyuncunun amacı anlaşılmıyor veya konu dışı konuşuyor.
ACTION:
- heal + hasMemoryOrb=true -> allow_entry
- heal + hasMemoryOrb=false -> request_more_info
- harmful -> deny
- unclear -> request_more_info
Action seçerken yalnızca bu tabloyu kullan.
REPLY:
- allow_entry: oyuncuya merkeze geçebileceğini söyle.
- deny: merkeze geçmesine izin verme. İpucu verme.
- unclear: oyuncudan amacını netleştirmesini iste. İpucu verme.
- heal + hasMemoryOrb=false durumunda hintLevel kullan.
- hintLevel=0: kalbin yeniden güzel bir şeyi hatırlaması gerektiğini sezdir.
- hintLevel=1: geçmişte iyi hissettiren bir anının izinin çevrede bulunabileceğini daha açık söyle.
- hasMemoryOrb=true ise hintLevel değerini tamamen yok say.
ÜSLUP:
- 1 veya 2 kısa cümle kullan.
- Doğal Türkçe kullan.
- Sakin, ciddi ve kararlı konuş.
- Aşırı resmi, mekanik veya şiirsel konuşma.
- Oyuncunun mesajını tekrar etme.
OYUN DURUMU:
hasMemoryOrb=<true|false>
hintLevel=<0|1>
OYUNCU MESAJI:
<oyuncunun yazdığı metin>
```

Muhafızın kişiliği sadece reply üslubunu etkiler. Kapının açılıp açılmayacağı (action) ayrıca kodda doğrulanır (bkz. 3.4).

### 3.3 Yapılandırılmış Çıktı Şeması

OllamaClient, Ollama'nın format alanına şu JSON şemasını verir; model bu şemaya uygun çıktı üretmeye zorlanır:

```json
{
  "type": "object",
  "properties": {
    "reply": { "type": "string" },
    "intent": { "type": "string", "enum": ["heal", "harmful", "unclear"] },
    "action": {
      "type": "string",
      "enum": ["allow_entry", "request_more_info", "deny"]
    }
  },
  "required": ["reply", "intent", "action"]
}
```

Gelen metin GuardLlmResponse sınıfına (reply, intent, action) ayrıştırılır. intent ve action değerleri GuardIntentParser / GuardActionParser ile GuardIntent / GuardAction enum'larına çevrilir.

### 3.4 Doğrulama ve Oyun Durumuna Bağlanma

GuardResponseValidator, modelin cevabını oyun durumuyla tutarlılık açısından kontrol eder:

| **intent** | **hasMemoryOrb** | **Geçerli action** |
| ---------- | ---------------- | ------------------ |
| harmful    | fark etmez       | deny               |
| unclear    | fark etmez       | request_more_info  |
| heal       | true             | allow_entry        |
| heal       | false            | request_more_info  |

Boş alan, bilinmeyen intent ya da tabloya uymayan action varsa cevap geçersiz sayılır. Geçerli cevapta:

- heal + request_more_info + küre yoksa GameState.IncreaseGuardHintLevel() çağrılır (en fazla 1), sonraki cevap daha açık ipucu verir.

- allow_entry ise GuardActionHandler DoorController.Open() ile kapıyı açar ve GuardInteraction.DisableInteraction() ile diyaloğu kapatır.

### 3.5 İstek / Yanıt Akışı

```mermaid
sequenceDiagram
actor P as Oyuncu
participant UI as DialogueUI
participant GC as GuardController
participant PB as GuardPromptBuilder
participant OC as OllamaClient (ILlmClient)
participant O as Ollama (gemma3:4b)
participant V as GuardResponseValidator
participant AH as GuardActionHandler
participant D as DoorController
P->>UI: Mesaj yazar, gönderir
UI->>UI: "Düşünüyor..." göster, giriş alanlarını kilitle
UI->>GC: SendMessageAsync(mesaj)
GC->>PB: BuildPrompt(mesaj, hasMemoryOrb, hintLevel)
GC->>OC: SendPromptAsync(prompt)
OC->>O: POST /api/generate (format: JSON şeması)
O-->>OC: response (JSON metin)
OC-->>GC: JSON metin
GC->>GC: JsonUtility.FromJson GuardLlmResponse
GC->>V: IsValid(cevap, hasMemoryOrb)
alt Geçerli
GC->>GC: Intent ve Action enum'a çevir
GC->>AH: HandleAction(action)
opt action = AllowEntry
AH->>D: Open()
end
GC-->>UI: GuardLlmResponse
UI-->>P: Muhafızın reply metnini göster
else Geçersiz / boş / ayrıştırılamadı
GC-->>UI: null
UI-->>P: Hata mesajı göster
end
```

### 3.6 Asenkron Yapı

- İstek async/await ile atılır (Task\<string\> SendPromptAsync), ana thread bloklanmaz.

- Bekleme sırasında arayüzde "Düşünüyor..." yazar, gönder butonu ve giriş alanı kilitlenir (çift gönderim engellenir).

- DialogueUI, her diyalog için bir dialogueVersion tutar. Diyalog kapanmışsa gecikmeli gelen eski cevap ekrana yazılmaz.

### 3.7 Muhafız diyalog ekranı

![](docs/images/e-ile-muhafız-sohbetini-acma.png)

## 4. Sistem Mimarisi ve OOP Tasarımı

### 4.1 Proje Yapısı

```text
Assets/Scripts/

├── Cat/ CatStateMachine, CatState, CatSensor, CatWallScanner, ObstacleWallMarker, CatAnimationController

├── Core/ GameManager, GameState, EndGameManager

├── Enemy/ EnemyPatrol, EnemyChase, EnemyDetection, EnemyDetectionZone, EnemyCatch

├── Guard/

│ ├── Enums/ GuardIntent, GuardAction

│ ├── LLM/ ILlmClient, OllamaClient, GuardPromptBuilder, GuardResponseSchema,

│ │ GuardLlmResponse, GuardResponseValidator, GuardIntentParser, GuardActionParser,

│ │ OllamaGenerateRequest, OllamaGenerateResponse

│ ├── GuardController, GuardActionHandler, GuardBootstrapper, DialogueMovementLock

├── Interactions/ IInteractable, PlayerInteraction, GuardInteraction, SphereInteraction,

│ SphereFloating, CatButtonTrigger

├── Player/ PlayerController, CameraFollow, PlayerInventory, PlayerSpawner

├── UI/ DialougeUI (DialogueUI), MainMenuManager, GameMenuController,

│ HowToPlayPanelController, GameExit

└── World/ DoorController, BreakableWall, DeathPit, HeartLightController,

HeartPulseLight, MainSceneNavMeshInitializer
```

### 4.2 Modüller ve sorumlulukları

Unity/C# kodları görev alanına göre **sekiz temel modüle** ayrılmıştır:

| **Modül**     | **Temel sınıflar**                                                                                       | **Sorumluluk**                                                                     |
| ------------- | -------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| Cat/          | CatStateMachine, CatState, CatSensor, CatWallScanner, CatAnimationController                             | FSM, NavMesh hareketi, Raycast ile duvar/geçit algılama, buton arama ve animasyon. |
| Core/         | GameManager, GameState, EndGameManager                                                                   | Oyun durumu, küre envanter bilgisi ve oyun sonu.                                   |
| Enemy/        | EnemyPatrol, EnemyChase, EnemyDetection, EnemyDetectionZone, EnemyCatch                                  | Devriye, algılama, takip ve yakalama.                                              |
| Guard/        | GuardController, GuardBootstrapper, GuardActionHandler, ILlmClient, OllamaClient, GuardResponseValidator | NPC diyaloğu, Ollama haberleşmesi, yanıt doğrulama, kapı açma eylemi.              |
| Interactions/ | IInteractable, PlayerInteraction, GuardInteraction, SphereInteraction, CatButtonTrigger                  | Oyuncu-NPC/küre etkileşimi ve kedinin butonu tetiklemesi.                          |
| Player/       | PlayerController, PlayerInventory, CameraFollow, PlayerSpawner                                           | Oyuncu hareketi, envanter, kamera ve spawn.                                        |
| UI/           | DialogueUI, MainMenuManager                                                                              | Diyalog arayüzü ve ana menü; diğer UI sınıfları yukarıdaki proje ağacındadır.      |
| World/        | BreakableWall, DoorController, DeathPit, HeartLightController, HeartPulseLight                           | Duvar, kapı, çukur ve kalp merkezi ışık/efektleri.                                 |

### 4.3 OOP ve SOLID prensipleri

Aşağıdaki tabloda iki kaynakta anlatılan prensipler, tekrara düşmeden aynı yerde gösterilmiştir.

| **Prensip**                                  | **Projedeki karşılığı**                                                                                                                                                                                                                                                            |
| -------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Kapsülleme (Encapsulation)**               | GameState içindeki oyun durumuna kontrollü erişim sağlanır; kürenin alınması CollectMemoryOrb() ile işlenir.                                                                                                                                                                       |
| **Soyutlama (Abstraction)**                  | ILlmClient arayüzünü OllamaClient uygular; GuardController LLM işlemlerini somut istemciden bağımsız yürütür.                                                                                                                                                                      |
| **Çok biçimlilik (Polymorphism)**            | PlayerInteraction, etkileşim nesnesini somut sınıfa değil IInteractable arayüzüne göre tutar; yeni etkileşimli nesneler bu arayüzü uygulayarak eklenebilir. Şu an arayüzü GuardInteraction uygular.                                                                                |
| **Kalıtım (Inheritance)**                    | PlayerController, CatStateMachine ve GuardController gibi bileşenler MonoBehaviour sınıfından türetilmiştir.                                                                                                                                                                       |
| **Tek sorumluluk (SRP)**                     | CatSensor ve CatWallScanner algılama işini; CatStateMachine karar mantığını yürütür. GuardPromptBuilder promptu kurar, GuardResponseValidator yanıtı doğrular, GuardIntentParser / GuardActionParser ayrıştırır, GuardActionHandler eylemi uygular, GameState oyun durumunu tutar. |
| **Arayüz ayrımı (ISP)**                      | ILlmClient ve IInteractable belirli amaçları olan küçük arayüzlerdir.                                                                                                                                                                                                              |
| **Bağımlılıkların tersine çevrilmesi (DIP)** | GuardController doğrudan OllamaClient'a değil ILlmClient'a bağımlıdır. GuardBootstrapper, istemciyi Initialize(ILlmClient) ile aktarır.                                                                                                                                            |
| **Kompozisyon**                              | CatStateMachine, CatSensor ile CatWallScanner nesnelerini birlikte kullanır; algılama ve karar ayrıdır.                                                                                                                                                                            |
| **Enum ile tip güvenliği**                   | CatState, GuardIntent, GuardAction kullanılır.                                                                                                                                                                                                                                     |
| **Bileşen bağımsızlığı**                     | Cat/ ve Guard/ birbirinin sınıflarını doğrudan kullanmaz; ayrı sorumluluklar üstlenirler.                                                                                                                                                                                          |

## 5. Hata Yönetimi

### 5.1 Oyun dünyası, otonom kedi ve NavMesh

- **Duvar / geçit bulunamaması:** CatStateMachine ve CatWallScanner geçerli duvar veya delik bulamazsa kedi oyuncuyu takip etmeye döner.

- **Buton ve keşif noktası bulunamaması:** CatSensor NavMesh erişilebilirliğini kontrol eder; ulaşılamayan noktaları eler. Yeni keşif noktası kalmadığında arama durur ve kedi takibe döner.

- **Düşman:** EnemyCatch, yakalama durumunda oyuncu ile kediyi başlangıç noktalarına döndürür. EnemyPatrol geçersiz devriye noktaları / ulaşılamayan hedefler için uyarı üretir.

- **Çukur:** DeathPit, oyuncu ve kediyi yeniden konumlandırır; kedinin NavMesh'e uygun yerleştirilememesi durumunda uyarı verir.

- **Duvar kırılması sonrası NavMesh:** BreakableWall, gerekli bileşenler bulunmazsa işlemi durdurup uyarı mesajı verir. Duvar kırıldıktan sonra NavMesh asenkron güncellenir; açılan koridor gezinilebilir olur.

### 5.2 Yerel LLM, JSON ve ağ hataları

| **Durum**                           | **Kodda ne olur**                                                                              | **Oyuncuya gösterilen**                                       |
| ----------------------------------- | ---------------------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| Ollama kapalı / bağlantı kurulamadı | HttpClient istisna fırlatır, DialogueUI genel catch ile yakalar, hata loglanır                 | "Muhafıza şu anda ulaşılamıyor. Lütfen tekrar dene."          |
| Zaman aşımı (15 sn)                 | TaskCanceledException yakalanır                                                                | "Muhafızın yanıt vermesi çok uzun sürdü. Lütfen tekrar dene." |
| Başarısız HTTP durumu               | OllamaClient durum kodu ve cevapla birlikte HttpRequestException fırlatır, genel catch yakalar | "Muhafıza şu anda ulaşılamıyor. Lütfen tekrar dene."          |
| Boş cevap                           | GuardController null döndürür                                                                  | "Muhafız geçerli bir yanıt üretemedi. Lütfen tekrar dene."    |
| Bozuk / ayrıştırılamayan JSON       | JsonUtility istisnası yakalanır, ham cevap loglanır, null döner                                | "Muhafız geçerli bir yanıt üretemedi. Lütfen tekrar dene."    |
| Tutarsız cevap (validator reddi)    | null döner                                                                                     | "Muhafız geçerli bir yanıt üretemedi. Lütfen tekrar dene."    |
| Bilinmeyen intent / action          | Parser false döner, null döner                                                                 | "Muhafız geçerli bir yanıt üretemedi. Lütfen tekrar dene."    |
| Boş oyuncu mesajı                   | Gönderilmez                                                                                    | "Lütfen bir mesaj yaz"                                        |

```mermaid
flowchart TD
A[LLM isteği] --> B{Sunucuya ulaşıldı mı?}
B -- Hayır --> E1[Ulaşılamıyor mesajı]
B -- Evet --> C{15 sn içinde cevap geldi mi?}
C -- Hayır --> E2[Zaman aşımı mesajı]
C -- Evet --> D{HTTP başarılı mı?}
D -- Hayır --> E1
D -- Evet --> F{JSON ayrıştı ve validator geçti mi?}
F -- Hayır --> E3[Geçerli yanıt üretilemedi mesajı]
F -- Evet --> G[Cevabı göster ve eylemi uygula]
E1 --> H[Oyun devam eder, oyuncu tekrar deneyebilir]
E2 --> H
E3 --> H
```

## 6. Ekran Görüntüleri, Test Senaryoları ve Sonuçlar

### 6.1 Otonom Kedi ve NavMesh Testleri

Bu bölümde, geliştirilen otonom kedi karakterinin engel aşma, yol bulma ve hedefe ulaşma performansı değerlendirilmiştir. Testler, kedinin farklı başlangıç konumlarında çevresini algılama ve görevini bağımsız olarak tamamlayabilme yeteneğini ölçmek amacıyla gerçekleştirilmiştir.

Test sürecinde kediye E tuşuyla komut verilmiş, ardından kedinin Raycast sensörleri aracılığıyla duvarı algılaması, uygun geçiş noktalarını belirlemesi ve kendisine en yakın geçidi seçmesi gözlemlenmiştir. Kedi, NavMesh yol bulma sistemini kullanarak engelin diğer tarafına geçmiş ve butonu aramaya başlamıştır. Arama sırasında daha önce ziyaret ettiği noktaları dikkate alarak yeni bölgeleri keşfetmiş, butonu tespit ettikten sonra hedefe ilerleyerek duvarın parçalanmasını sağlamıştır.

Kedinin farklı başlangıç konumlarındaki performansını değerlendirmek amacıyla sol, orta ve sağ olmak üzere üç farklı konumda beşer deneme yapılmıştır.

| **Başlangıç konumu** | **Deneme sayısı** | **Başarı oranı** | **Ortalama süre** |
| -------------------- | ----------------- | ---------------- | ----------------------------------- |
| Sol                  | 5                 | %100             | 6 s                                  |
| Orta                 | 5                 | %100             | 5 s                                  |
| Sağ                  | 5                 | %80              | 7 s                                  |

#### 6.1.1 Kedinin davranışına ait kanıtlar

**Görsel kanıt alanı:** Oyuncu ile kedinin duvar önünde beklemesi / `E` tuşu logu — ekran görüntüleri ve Console kayıtları

![Test Log 1](docs/images/log1.png)
![Kedi Görsel 1](docs/images/cat1.png)

**Görsel kanıt alanı:** Kedinin duvarı Raycast ile bulması, geçitleri taraması ve en yakın geçidi seçmesi — ekran görüntüleri ve Console kayıtları   

![Test Log 2](docs/images/log2.png)
![Test Log 2](docs/images/log2.1.png)
![Test Log 2](docs/images/log2.2.png)
![Kedi Görsel 2](docs/images/cat2.png)

**Görsel kanıt alanı:** Kedinin geçidi aşması — ekran görüntüleri ve Console kayıtları

![Test Log 3](docs/images/log3.png)
![Kedi Görsel 3](docs/images/cat3.png)


**Görsel kanıt alanı:** NavMesh ile yeni bölgeleri keşfederek butonu araması — ekran görüntüleri ve Console kayıtları

![Test Log 4](docs/images/log4.png)
![Kedi Görsel 4](docs/images/cat4.png)

**Görsel kanıt alanı:** Butonun algılanması, etkinleştirilmesi ve duvarın parçalanması — ekran görüntüleri ve Console kayıtları

![Test Log 5](docs/images/log5.png)
![Test Log 5.1](docs/images/log5.1.png)
![Kedi Görsel 5](docs/images/cat5.png)

### 6.2 Oyun Mekanikleri Testleri

Düşman, çukur, küre alma/yerleştirme ve yeniden başlatma mekanizmaları test edilmiştir.

| **Test senaryosu**     | **Beklenen sonuç**              | **Test sonucu** |
| ---------------------- | ------------------------------- | ---------------------------------- |
| Düşmana temas          | Oyuncu ve kedi başlangıca döner | Olumlu                             |
| Çukura düşme           | Oyuncu ve kedi başlangıca döner | Olumlu                             |
| Düşmandan kaçış        | Oyuncu ilerlemeye devam eder    | Olumlu                             |
| Kalp küresini alma     | Küre oyuncu tarafından alınır   | Olumlu                             |
| Küreyi merkeze bırakma | Oyun tamamlanır                 | Olumlu                             |

**Görsel kanıt alanı:** Test 1 — Düşmana yakalanma — mevcut görüntüler

![Düşman Testi 1](docs/images/enemy1.png)
![Düşman Testi 2](docs/images/enemy2.png)

**Görsel kanıt alanı:** Test 2 — Çukura düşme — mevcut görüntüler

![Çukur Testi 1](docs/images/çukur1.png)
![Çukur Testi 2](docs/images/çukur2.png)

**Görsel kanıt alanı:** Test 4 — Kalp küresini alma — mevcut görüntüler

![Görsel küre](docs/images/küre1.png)
![Görsel küre2](docs/images/küre2.png)

**Görsel kanıt alanı:** Şekil 11 — Kürenin kalbin merkezine yerleştirilmesi — mevcut görüntüler

![Kalp Merkezi Logu](docs/images/logcenter.png)
![Kalbin Merkezi](docs/images/center1.png)

**Görsel kanıt alanı:** Şekil 12 — Görevin tamamlanması ve oyun bitiş ekranı — mevcut görüntüler

![Oyun Sonu Logu](docs/images/logend.png)
![Oyun Bitiş Ekranı](docs/images/end.png)

### 6.3 LLM Diyalog Testleri

| # | Senaryo | hasMemoryOrb | intent / action | Kapı açıldı mı |
|---|---------|--------------|-----------------|----------------|
| 1 | İyi niyetli | true | heal / allow_entry | Evet |
| 2 | Kötü niyetli | true | harmful / deny | Hayır |
| 3 | Alakasız | true | unclear / request_more_info | Hayır |
| 4 | Caydırıcı | true | unclear / request_more_info | Hayır |
| 5 | Alaycı | true | harmful / deny | Hayır |
| 6 | İyi niyetli | false | heal / request_more_info | Hayır |
| 7 | Kötü niyetli | false | harmful / deny | Hayır |
| 8 | Alakasız | false | unclear / request_more_info | Hayır |
| 9 | Caydırıcı | false | unclear / request_more_info | Hayır |
| 10 | Alaycı | false | unclear / request_more_info | Hayır |

> **Not:** Intent sınıflandırmasını model yaptığı için, alaycı gibi belirsiz tonlu mesajlar aynı senaryoda farklı intent'e (harmful / unclear) dönüşebilir. Action ise intent ve `hasMemoryOrb` değerine göre kodda doğrulanır (bkz. 3.4), bu yüzden kapının açılıp açılmaması etkilenmez.

**Test 1 — İyi niyetli, küre var**

<img src="docs/images/kure-iyi-niyet.png" width="900">
<img src="docs/images/kure-iyi-niyet-kapı.png" width="900">

**Test 2 — Kötü niyetli, küre var**

<img src="docs/images/kure-kötü-niyet.png" width="900">

**Test 3 — Alakasız, küre var**

<img src="docs/images/kure-alakasız.png" width="900">

**Test 4 — Caydırıcı, küre var**

<img src="docs/images/kure-caydırıcı.png" width="900">

**Test 5 — Alaycı, küre var**

<img src="docs/images/kure-alaycı.png" width="900">

**Test 6 — İyi niyetli, küre yok**

<img src="docs/images/kuresiz-iyi-niyet.png" width="900">

**Test 7 — Kötü niyetli, küre yok**

<img src="docs/images/kuresiz-kotu-niyet.png" width="900">

**Test 8 — Alakasız, küre yok**

<img src="docs/images/kuresiz-alakasız.png" width="900">

**Test 9 — Caydırıcı, küre yok**

<img src="docs/images/kuresiz-caydırıcı.png" width="900">

**Test 10 — Alaycı, küre yok**

<img src="docs/images/kuresiz-alaycı.png" width="900">

## 7. Sonuç ve Tartışma

Bu projede Unity/C# üzerinde **NavMesh**, **Raycast**, **Sonlu Durum Makinesi (FSM)**, **yerel LLM**, **OOP** ve oyun durumunu etkileyen yapılandırılmış yanıt işleme yöntemleri bir araya getirilmiştir. Otonom kedinin geçidi ve butonu keşfetmesi ile muhafızın serbest metin üzerinden dinamik yanıt vermesi, iki farklı yapay zekâ bileşeninin tek 3D macera ortamında birlikte kullanılmasını sağlamıştır.

### Başarılar

- Kedi, sabit koordinat yerine Raycast sensörleri ve NavMesh sorgularıyla duvardaki geçidi ve butonu çalışma zamanında bulur.

- LLM çıktısı JSON şemasına bağlanmıştır; ayrıca kodda doğrulanır ve enum'lara çevrilir.

- Sunucu hatası, zaman aşımı ve bozuk cevap durumlarında oyun çalışmaya devam eder.

- 3 başlangıç konumunun testlerinde sırasıyla %100, %100 ve %80 başarı oranı raporlanmıştır.

- 5 oyun mekaniği testinin tamamı "olumlu" olarak kaydedilmiştir.


### Sınırlılıklar

- Model adı ve Ollama adresi OllamaClient içinde sabit (const) olarak tanımlıdır.

- Hatalı cevapta otomatik yeniden deneme yoktur.

- Kedinin durum makinesi switch tabanlı tek sınıftadır (CatStateMachine).

### Olası Geliştirmeler

- **Otonom ajan:** ML-Agents kullanılarak pekiştirmeli öğrenme (RL) tabanlı companion ve daha karmaşık kedi görevleri.

- **LLM altyapısı:** Model adı ile sunucu adresinin ayar dosyasından okunması ve kontrollü otomatik yeniden deneme.

- **Oyun haritası:** Alternatif yollar, yeni engeller ve farklı güzergâhlar.

- **Companion seçimi:** Başlangıçta farklı özelliklerde kediler arasından seçim yapabilme.

- **Yeni mekanikler:** Oyuncuya çeşitli silahlar veya ekipmanlar verilerek kalbin damarlarındaki mikroplarla mücadele edebilme.

- **Çeşitlilik:** Farklı düşman türleri, yeni çevre etkileşimleri ve daha karmaşık görevler.

## 8. Kaynakça ve Kullanılan Assetler

### 8.1 Geliştirme teknolojileri

1.  **Unity Technologies — Unity 6.6 / Unity Documentation.** Oyun motoru ve geliştirme ortamı: <https://docs.unity3d.com/>

2.  **Microsoft — Visual Studio Community 2022.** C# geliştirme ortamı: <https://learn.microsoft.com/tr-tr/visualstudio/>

3.  **Microsoft — C# Documentation.** C# ve OOP: <https://learn.microsoft.com/tr-tr/dotnet/csharp/>

4.  **Unity Technologies — AI Navigation Documentation.** NavMesh yol bulma: <https://docs.unity3d.com/Packages/com.unity.ai.navigation@2.0/manual/index.html>

5.  **Unity Technologies — Physics.Raycast Scripting API.** Raycast: <https://docs.unity3d.com/ScriptReference/Physics.Raycast.html>

6.  **Git ve GitHub — Sürüm kontrolü ve proje yönetimi:** <https://git-scm.com/doc> ve <https://docs.github.com/>

### 8.2 Yapay zekâ ve diyagram araçları

- **Ollama:** <https://ollama.com/>

- **Mermaid Documentation:** <https://mermaid.js.org/>

- **Mermaid Live Editor:** <https://mermaid.live/>

### 8.3 Kullanılan 3D assetler ve animasyonlar

- **Sketchfab — Living Flesh Blob (Free Organic Horror Asset):** Düşman karakter modeli. <https://sketchfab.com/3d-models/living-flesh-blob-free-organic-horror-asset-6357bbc094d54cf5ba150ef2951b3750>

- **Adobe Mixamo — Character Animations:** Oyuncunun yürüme, bekleme ve eğilme animasyonları. <https://www.mixamo.com/>

- **Ladymito — Free Chibi Cat:** Otonom kedi modeli (Unity Asset Store). <https://assetstore.unity.com/packages/3d/characters/animals/mammals/free-chibi-cat-165490>

- **Loafbrr — Mines and Cave Set:** Mağara ve çevre duvarları. <https://marketplace.unity.com/packages/3d/environments/mines-and-cave-set-305701>

- **Sketchfab — Crystal Heart:** Kalp modeli. <https://sketchfab.com/3d-models/crystal-heart-8e8515a0834b4c1e9e477ce0f392a90c>

- **Sketchfab — Round Stone Altar:** Kalp merkezindeki sunak. <https://sketchfab.com/3d-models/round-stone-altar-812c0b3927cb414698a1613d08e150b1>

- **Sketchfab — Sandstone Dome House:** Çevre modeli. <https://sketchfab.com/3d-models/sandstone-dome-house-0d5837a72ea54818903bed7ce50f42ef>

- **Sketchfab —Rigged Female Character: Leather Jacket & Boots:** Oyuncu (Player) için kullanılan kadın karakter modeli. <https://sketchfab.com/3d-models/rigged-female-character-leather-jacket-boots-05c240d89fb34089ad6491ba9499ad0c?>
   
- **Sketchfab — Pale Knight Animated (mara3m):** Kalp Muhafızı modeli. <https://skfb.ly/pBuOy> · Lisans: [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/)

- **Sketchfab — Medieval City Gate (Daan van Leeuwen):** Kalp Muhafızının koruduğu kapı modeli. <https://skfb.ly/6RoHp> · Lisans: [CC BY 4.0](http://creativecommons.org/licenses/by/4.0/)


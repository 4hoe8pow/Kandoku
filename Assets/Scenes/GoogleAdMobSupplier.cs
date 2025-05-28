using GoogleMobileAds.Api;
using System.Collections;
using UnityEngine;

/// <summary>
/// AdMob バナー／インタースティシャル統合サプライヤ
/// </summary>
public class GoogleAdMobSupplier : MonoBehaviour
{
    // ──────────────────────────────────────────────────────────────
    // Ad Unit IDs
    // ──────────────────────────────────────────────────────────────
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private static readonly string BannerId = "ca-app-pub-3940256099942544/6300978111"; // テスト
    private static readonly string InterstitialId = "ca-app-pub-3940256099942544/1033173712"; // テスト
#else
#if UNITY_ANDROID
        private static readonly string BannerId = "ca-app-pub-8110178142432057/7088727883"; // 本番(Android)
#else
        private static readonly string BannerId = "ca-app-pub-8110178142432057/2671912671"; // 本番(iOS)
#endif
    private static readonly string InterstitialId = "ca-app-pub-8110178142432057/6099780542"; // 本番
#endif

    // ──────────────────────────────────────────────────────────────
    // Internal fields
    // ──────────────────────────────────────────────────────────────
    private BannerView _bannerView;
    private InterstitialAd _interstitial;
    private static readonly AdRequest _defaultRequest = new();   // 使い回し

    private float interstitialInterval = 300f; // 5分
    private Coroutine interstitialCoroutine;

    // ──────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        // SDK 初期化が完了してから初回ロード
        MobileAds.Initialize(_ =>
        {
            LoadBannerAd();
            LoadInterstitialAd();
        });
    }

    private void Start()
    {
        // 初回だけ即表示（ロード済みなら）
        StartCoroutine(ShowInterstitialOnStart());
        // 5分ごとに広告表示
        interstitialCoroutine = StartCoroutine(ShowInterstitialRoutine());
    }

    private IEnumerator ShowInterstitialOnStart()
    {
        // 広告ロード完了まで待機（最大10秒）
        float timeout = 10f;
        float elapsed = 0f;
        while ((_interstitial == null || !_interstitial.CanShowAd()) && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }
        if (_interstitial != null && _interstitial.CanShowAd())
        {
            _interstitial.Show();
            Debug.Log("[AdMob] 初回インタースティシャル広告を表示しました");
        }
        else
        {
            Debug.Log("[AdMob] 初回インタースティシャル広告の表示に失敗または未ロード");
        }
    }

    private void OnDestroy()
    {
        DestroyBanner();
        _interstitial?.Destroy();
    }

    private IEnumerator ShowInterstitialRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(interstitialInterval);
            Debug.Log($"[AdMob] {interstitialInterval}秒ごとにインタースティシャル広告を表示します");
            if (_interstitial != null && _interstitial.CanShowAd())
            {
                _interstitial.Show();
                Debug.Log("[AdMob] インタースティシャル広告を表示しました");
            }
            else
            {
                Debug.Log("[AdMob] インタースティシャル広告未ロード。再ロードします");
                LoadInterstitialAd();
            }
        }
    }

    // ──────────────────────────────────────────────────────────────
    // Banner
    // ──────────────────────────────────────────────────────────────
    public void LoadBannerAd()
    {
        if (_bannerView == null) CreateBannerView();
        _bannerView.LoadAd(_defaultRequest);
    }

    private void CreateBannerView()
    {
        if (_bannerView != null) DestroyBanner();

        _bannerView = new BannerView(BannerId, AdSize.Banner, AdPosition.Top);

        _bannerView.OnBannerAdLoaded += () => _bannerView.Show();
        _bannerView.OnBannerAdLoadFailed += e =>
            Debug.LogError($"[AdMob][Banner:{BannerId}] Load error: {e.GetMessage()}");
    }

    private void DestroyBanner()
    {
        _bannerView?.Destroy();
        _bannerView = null;
    }

    // ──────────────────────────────────────────────────────────────
    // Interstitial
    // ──────────────────────────────────────────────────────────────
    /// <summary>
    /// 表示予定の 10〜15 秒前など、任意タイミングで呼び出してプリロード
    /// </summary>
    public void LoadInterstitialAd()
    {
        if (_interstitial != null && _interstitial.CanShowAd()) return; // 既に準備済み

        Debug.Log("[AdMob] Loading interstitial…");

        _interstitial?.Destroy();   // 旧インスタンス破棄

        InterstitialAd.Load(
            InterstitialId,
            _defaultRequest,
            (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"[AdMob][Interstitial:{InterstitialId}] Load failed: {error?.GetMessage()}");
                    _interstitial = null;
                    return;
                }

                _interstitial = ad;

                Debug.Log("[AdMob] インタースティシャル広告ロード完了");

                _interstitial.OnAdFullScreenContentClosed += () =>
                {
                    _interstitial.Destroy();
                    _interstitial = null;
                    // 次回用にプリロード
                    LoadInterstitialAd();
                };
            });
    }

    /// <summary>
    /// 準備できていれば即表示。未ロードならロードを試みつつ false を返す
    /// </summary>
    public bool ShowInterstitialAd()
    {
        if (_interstitial != null && _interstitial.CanShowAd())
        {
            _interstitial.Show();
            return true;
        }

        Debug.LogWarning("[AdMob] Interstitial not ready – loading now.");
        LoadInterstitialAd();
        return false;
    }
}

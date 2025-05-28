using GoogleMobileAds.Api;
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

    private void OnDestroy()
    {
        DestroyBanner();
        _interstitial?.Destroy();
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

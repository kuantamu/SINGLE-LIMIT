using System;
using System.Collections.Generic;
using UnityEngine;

#region
/// <summary>
/// 被弾時のキャラクターの反応状態を表す列挙型。
/// ダメージ受付・ノックバック可否の判定に使用する。
/// </summary>
public enum HitReactionState
{
    /// <summary>通常状態。ダメージ・ノックバックともに受け付ける。</summary>
    Normal,

    /// <summary>
    /// ダウン状態。ダメージを受け付けるが属性耐性が Weak 扱いになる。
    /// ノックバックも受け付ける。
    /// </summary>
    Down,

    /// <summary>
    /// スーパーアーマー状態。ダメージは受け付けるがノックバックを無効化する。
    /// </summary>
    Armor,

    /// <summary>
    /// 無敵状態（回避中など）。ダメージもノックバックも受け付けない。
    /// 視覚的なフィードバックとしてシアン色の点滅エフェクトが適用される。
    /// </summary>
    Invincible
}
#endregion
/// <summary>
/// キャラクターの実行時ステータスを管理する MonoBehaviour。
/// プレイヤー・敵ともにこのコンポーネントをアタッチして使用する。
///
/// ■ 使い方
///   1. <see cref="CharacterStatData"/> を Inspector の StatData に設定する
///   2. <see cref="TakeDamage"/> を呼ぶとダメージ計算・HP 更新・死亡判定が行われる
///   3. <see cref="OnDeath"/> イベントを購読して死亡時の処理を実装する
///
/// ■ ダメージバフ
///   <see cref="AddDamageBuff"/> で送受ダメージに乗算バフをかけることができる。
///   duration を指定すると時間経過で自動削除される。
///
/// ■ 被弾状態
///   <see cref="SetHitReactionState"/> で恒久的に変更し、
///   <see cref="SetTimedHitReactionState"/> で一時的な状態（無敵時間など）を設定できる。
/// </summary>
public class CharacterStats : MonoBehaviour
{
    #region 変数
    #region Inspector フィールド
    [Header("ステータスデータ")]
    /// <summary>キャラクターの基本ステータスを定義する ScriptableObject。</summary>
    [SerializeField] private CharacterStatData _statData;

    [Header("Faction")]
    /// <summary>このキャラクターの所属陣営。</summary>
    [SerializeField] private CharacterFaction _faction = CharacterFaction.Other;

    /// <summary>
    /// true の場合、<see cref="CharacterFactionRules"/> のデフォルト関係を使用する。
    /// false の場合、以下の _hostileFactions / _attackableFactions を直接参照する。
    /// </summary>
    [SerializeField] private bool _useDefaultFactionRelations = true;

    /// <summary>カスタム敵対陣営マスク（_useDefaultFactionRelations が false の場合に有効）。</summary>
    [SerializeField] private CharacterFactionMask _hostileFactions   = CharacterFactionMask.None;

    /// <summary>カスタム攻撃可能陣営マスク（_useDefaultFactionRelations が false の場合に有効）。</summary>
    [SerializeField] private CharacterFactionMask _attackableFactions = CharacterFactionMask.None;

    [Header("Hit Reaction Debug")]
    /// <summary>現在の被弾反応状態（Inspector からデバッグ変更可能）。</summary>
    [SerializeField] private HitReactionState _hitReactionState = HitReactionState.Normal;

    /// <summary>無敵状態の点滅に使用するハイライトカラー。</summary>
    [SerializeField] private Color _invincibleBlinkColor = Color.cyan;

    /// <summary>無敵状態の点滅速度（大きいほど速く点滅する）。</summary>
    [SerializeField] private float _invincibleBlinkSpeed = 12f;
    #endregion
    #region private フィールド
    /// <summary>現在有効なダメージバフのリスト。</summary>
    private readonly List<DamageBuff> _damageBuffs = new List<DamageBuff>();

    /// <summary>各 Renderer に対応する MaterialPropertyBlock のキャッシュ。</summary>
    private readonly List<MaterialPropertyBlock> _rendererBlocks = new List<MaterialPropertyBlock>();

    /// <summary>子オブジェクト含む Renderer のキャッシュ（Awake 時に取得）。</summary>
    private Renderer[] _renderers;

    /// <summary>マテリアル変更前のベースカラーのキャッシュ（無敵点滅のリセット用）。</summary>
    private Color[] _baseColors;

    /// <summary>前フレームに適用した被弾状態（変更検知用）。</summary>
    private HitReactionState _lastAppliedHitReactionState;

    /// <summary>一時的な被弾状態が終了した際に戻る元の状態。</summary>
    private HitReactionState _stateBeforeTimedState;

    /// <summary>一時的な被弾状態の残り時間（秒）。</summary>
    private float _timedStateTimer;

    /// <summary>一時的な被弾状態が有効かどうかのフラグ。</summary>
    private bool _hasTimedState;

    // URP (_BaseColor) と Built-in (_Color) の両シェーダーに対応するためのプロパティ ID キャッシュ
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId     = Shader.PropertyToID("_Color");
    #endregion
    #region 参照データ
    public CharacterStats lastAttackChara;

    /// <summary>キャラクターの基本ステータスデータ。</summary>
    public CharacterStatData StatData => _statData;

    /// <summary>このキャラクターの陣営。</summary>
    public CharacterFaction Faction => _faction;

    /// <summary>
    /// 敵対する陣営のマスク。
    /// <see cref="_useDefaultFactionRelations"/> が true の場合は
    /// <see cref="CharacterFactionRules.DefaultHostileFactions"/> を、
    /// false の場合は Inspector 設定値を返す。
    /// </summary>
    public CharacterFactionMask HostileFactions => _useDefaultFactionRelations
        ? CharacterFactionRules.DefaultHostileFactions(_faction)
        : _hostileFactions;

    /// <summary>
    /// 攻撃可能な陣営のマスク。
    /// <see cref="_useDefaultFactionRelations"/> が true の場合は
    /// <see cref="CharacterFactionRules.DefaultAttackableFactions"/> を、
    /// false の場合は Inspector 設定値を返す。
    /// </summary>
    public CharacterFactionMask AttackableFactions => _useDefaultFactionRelations
        ? CharacterFactionRules.DefaultAttackableFactions(_faction)
        : _attackableFactions;

    /// <summary>現在の陣営を返す（<see cref="Faction"/> プロパティの代替メソッド）。</summary>
    public CharacterFaction GetFaction() { return _faction; }

    /// <summary>カスタム敵対陣営マスクを返す（デフォルト関係を使用している場合でも内部フィールドをそのまま返す）。</summary>
    public CharacterFactionMask GetEnemyFaction() { return _hostileFactions; }

    /// <summary>
    /// StatData を差し替える。ステージ途中でのステータス変化（強化・変身など）に使用する。
    /// </summary>
    /// <param name="statData">新しいステータスデータ</param>
    /// <param name="resetHp">true の場合、HP を新しい MaxHP にリセットする</param>
    public void SetStatData(CharacterStatData statData, bool resetHp)
    {
        if (statData == null) return;

        _statData = statData;
        if (resetHp)
            CurrentHP = _statData.MaxHP;
    }

    /// <summary>現在の HP。</summary>
    public int CurrentHP { get; private set; }

    /// <summary>最大 HP（StatData が null の場合は 0）。</summary>
    public int MaxHP => _statData != null ? _statData.MaxHP : 0;

    /// <summary>HP が 0 以下のとき true を返す。</summary>
    public bool IsDead => CurrentHP <= 0;

    /// <summary>
    /// 現在ガードアクション中かどうか。
    /// GuardState.Enter で true、GuardState.Exit で false に設定する。
    /// </summary>
    public bool IsGuarding { get; set; }

    /// <summary>現在の被弾反応状態。</summary>
    public HitReactionState CurrentHitReactionState => _hitReactionState;

    /// <summary>無敵状態かどうか。</summary>
    public bool IsInvincible => _hitReactionState == HitReactionState.Invincible;

    /// <summary>スーパーアーマー状態かどうか。</summary>
    public bool IsArmor => _hitReactionState == HitReactionState.Armor;

    /// <summary>ダウン状態かどうか。</summary>
    public bool IsDown => _hitReactionState == HitReactionState.Down;

    /// <summary>ヒットを受け付けられるか（生存中かつ無敵でない場合に true）。</summary>
    public bool CanReceiveHit => !IsDead && !IsInvincible;

    /// <summary>ノックバックを受け付けられるか（生存中かつ無敵・アーマーでない場合に true）。</summary>
    public bool CanBeKnockedBack => !IsDead && !IsInvincible && !IsArmor;

    /// <summary>指定キャラクターに対して敵対しているかを返す。</summary>
    public bool IsHostileTo(CharacterStats target) => CharacterFactionRules.IsHostile(this, target);

    /// <summary>指定キャラクターを攻撃できるかを返す。</summary>
    public bool CanAttack(CharacterStats target) => CharacterFactionRules.CanAttack(this, target);

    /// <summary>
    /// 送り出すダメージへの乗算倍率。
    /// <see cref="DamageBuffTarget.OutgoingDamage"/> のバフをすべて掛け合わせた値。
    /// </summary>
    public float OutgoingDamageMultiplier => GetDamageMultiplier(DamageBuffTarget.OutgoingDamage);

    /// <summary>
    /// 受け取るダメージへの乗算倍率。
    /// <see cref="DamageBuffTarget.IncomingDamage"/> のバフをすべて掛け合わせた値。
    /// </summary>
    public float IncomingDamageMultiplier => GetDamageMultiplier(DamageBuffTarget.IncomingDamage);

    /// <summary>
    /// ダメージを受けた直後に発火する。
    /// 引数: (実ダメージ量, クリティカルヒットか, 攻撃の属性タイプ)
    /// </summary>
    public event Action<int, bool, AttributeType> OnDamaged;

    /// <summary>HP が 0 になった際に発火する。死亡演出やドロップ処理などを実装する際に購読する。</summary>
    public event Action OnDeath;

    /// <summary>ダメージバフが追加・削除・期限切れになった際に発火する。UI 更新などに利用する。</summary>
    public event Action OnDamageBuffsChanged;
    #endregion
    #endregion

    private void Awake()
    {
        // Renderer を先にキャッシュしてベースカラーを保存する
        CacheRenderers();

        if (_statData == null)
        {
            Debug.LogWarning($"[CharacterStats] {gameObject.name} に StatData が設定されていません。");
            return;
        }

        // HP を最大値で初期化する
        CurrentHP = _statData.MaxHP;
    }

    private void Update()
    {
        // ダメージバフの時間経過処理（期限切れバフを削除する）
        TickDamageBuffs(Time.deltaTime);

        // 一時的な被弾状態のタイマーを更新し、期限切れなら元の状態に戻す
        TickTimedHitReactionState(Time.deltaTime);

        // 被弾状態に応じたビジュアル（無敵点滅など）を更新する
        UpdateHitReactionVisual();
    }

    #region 陣営自動設定関連
    /// <summary>
    /// Inspector での値変更時に呼ばれる Unity コールバック。
    /// デフォルト関係を使用している場合、Inspector 上の陣営フィールドを
    /// 設定の値に自動更新してプレビュー表示に反映する。
    /// </summary>
    private void OnValidate()
    {
        if (!_useDefaultFactionRelations) return;

        _hostileFactions   = CharacterFactionRules.DefaultHostileFactions(_faction);
        _attackableFactions = CharacterFactionRules.DefaultAttackableFactions(_faction);
    }
    #endregion
    #region ダメージを受ける処理
    /// <summary>
    /// ダメージを受ける処理。<c>DamageHitEffect</c> などの攻撃判定スクリプトから呼び出す。
    /// 以下の順で処理を行う。
    /// <list type="number">
    ///   <item>死亡・無敵チェック（受け付けない場合は即リターン）</item>
    ///   <item>ガード中フラグ・受信ダメージ倍率を DamageInfo に反映</item>
    ///   <item><see cref="DamageCalculator.Calculate"/> でダメージ値を算出</item>
    ///   <item>HP を減算し <see cref="OnDamaged"/> を発火</item>
    ///   <item>HP が 0 になった場合は <see cref="OnDeath"/> を発火</item>
    /// </list>
    /// </summary>
    /// <param name="info">攻撃側が設定するダメージ情報</param>
    public void TakeDamage(DamageInfo info)
    {
        lastAttackChara = info.AttackChara;

        if (IsDead)        return; // 既に死亡済みなら処理しない
        if (_statData == null) return; // StatData 未設定なら処理しない
        if (!CanReceiveHit) return; // 無敵状態なら処理しない

        // ガード状態・受信ダメージ倍率を DamageInfo に設定する
        info.IsGuarded                  = IsGuarding;
        info.IncomingDamageMultiplier   = IncomingDamageMultiplier;
        info.UseIncomingDamageMultiplier = true;

        // ダウン中は属性耐性を強制的に弱点扱いにする
        info.ForceWeakAttribute = IsDown;

        // ダメージ量を算出（クリティカル判定も含む）
        int damage = DamageCalculator.Calculate(info, _statData, out bool isCritical);

        // HP を減算（0 未満にはしない）
        CurrentHP = Mathf.Max(0, CurrentHP - damage);

        // HP 更新後にダメージイベントを発火する
        OnDamaged?.Invoke(damage, isCritical, info.Attribute);

        // 死亡判定
        if (CurrentHP <= 0)
            OnDeath?.Invoke();
    }
    #endregion
    #region
    /// <summary>
    /// 被弾反応状態を恒久的に変更する。一時的な状態タイマーはリセットされる。
    /// Inspector デバッグや特定のゲームイベントで状態を固定する際に使用する。
    /// </summary>
    /// <param name="state">設定する被弾反応状態</param>
    public void SetHitReactionState(HitReactionState state)
    {
        _hasTimedState   = false;
        _timedStateTimer = 0f;
        _hitReactionState = state;
    }
    #endregion
    #region
    /// <summary>
    /// 一時的な被弾反応状態を設定する。<paramref name="duration"/> 秒後に元の状態へ自動復帰する。
    /// 回避無敵・怯み・ダウンなどの時限的な状態変化に使用する。
    /// duration が 0 以下の場合は恒久変更として扱う。
    /// </summary>
    /// <param name="state">一時的に適用する被弾反応状態</param>
    /// <param name="duration">状態を維持する秒数</param>
    public void SetTimedHitReactionState(HitReactionState state, float duration)
    {
        if (duration <= 0f)
        {
            SetHitReactionState(state);
            return;
        }

        // 既に一時状態中の場合は復帰先を変えず、タイマーのみ上書きする
        _stateBeforeTimedState = _hasTimedState ? _stateBeforeTimedState : _hitReactionState;
        _hitReactionState      = state;
        _timedStateTimer       = duration;
        _hasTimedState         = true;
    }
    #endregion
    #region
    /// <summary>
    /// 実効的な属性耐性レベルを返す。
    /// ダウン状態の場合はすべての属性に対して <see cref="ResistanceLevel.Weak"/> を返す。
    /// </summary>
    /// <param name="attribute">照会する属性タイプ</param>
    /// <returns>実際に適用される耐性レベル</returns>
    public ResistanceLevel GetEffectiveAttributeResistanceLevel(AttributeType attribute)
    {
        // ダウン中は全属性が弱点扱い
        if (IsDown) return ResistanceLevel.Weak;
        return _statData != null ? _statData.GetAttributeResistanceLevel(attribute) : ResistanceLevel.Normal;
    }
    #endregion
    #region
    /// <summary>
    /// ダメージバフを追加する。同一 ID のバフが既に存在する場合はリフレッシュする。
    /// </summary>
    /// <param name="target">バフの適用対象（送ダメージ or 受ダメージ）</param>
    /// <param name="multiplier">ダメージ乗算倍率</param>
    /// <param name="duration">有効期間（秒）。-1 の場合は無期限。</param>
    /// <param name="id">バフを一意に識別する ID。null の場合は重複チェックを行わない。</param>
    /// <param name="overwriteMultiplier">同一 ID 存在時に倍率を上書きするか</param>
    /// <param name="overwriteDuration">同一 ID 存在時に期間を上書きするか</param>
    /// <returns>追加または更新されたバフのインスタンス</returns>
    public DamageBuff AddDamageBuff(
        DamageBuffTarget target,
        float multiplier,
        float duration          = -1f,
        string id               = null,
        bool overwriteMultiplier = true,
        bool overwriteDuration   = true)
    {
        // 同一 ID のバフが既にある場合はリフレッシュして返す
        DamageBuff existing = FindDamageBuff(id);
        if (existing != null)
        {
            existing.Refresh(multiplier, duration, overwriteMultiplier, overwriteDuration);
            OnDamageBuffsChanged?.Invoke();
            return existing;
        }

        // 新規バフを追加する
        DamageBuff buff = new DamageBuff(id, target, multiplier, duration);
        _damageBuffs.Add(buff);
        OnDamageBuffsChanged?.Invoke();
        return buff;
    }
    #endregion
    #region
    /// <summary>
    /// 指定 ID のダメージバフを削除する。
    /// </summary>
    /// <param name="id">削除するバフの ID</param>
    /// <returns>バフが見つかり削除された場合は <c>true</c></returns>
    public bool RemoveDamageBuff(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        // 末尾から検索して削除（リストのシフトコストを最小化）
        for (int i = _damageBuffs.Count - 1; i >= 0; i--)
        {
            if (_damageBuffs[i].Id != id) continue;

            _damageBuffs.RemoveAt(i);
            OnDamageBuffsChanged?.Invoke();
            return true;
        }

        return false;
    }
    #endregion
    #region
    /// <summary>
    /// 有効なダメージバフをすべて削除する。
    /// </summary>
    public void ClearDamageBuffs()
    {
        if (_damageBuffs.Count == 0) return;

        _damageBuffs.Clear();
        OnDamageBuffsChanged?.Invoke();
    }
    #endregion
    // -------------------------------------------------------
    // プライベートメソッド
    // -------------------------------------------------------
    #region
    /// <summary>
    /// ダメージバフのタイマーを更新し、期限切れのバフを削除する。
    /// </summary>
    /// <param name="deltaTime">経過時間（秒）</param>
    private void TickDamageBuffs(float deltaTime)
    {
        if (_damageBuffs.Count == 0) return;

        bool changed = false;
        for (int i = _damageBuffs.Count - 1; i >= 0; i--)
        {
            _damageBuffs[i].Tick(deltaTime);
            if (!_damageBuffs[i].IsExpired) continue;

            _damageBuffs.RemoveAt(i);
            changed = true;
        }

        if (changed)
            OnDamageBuffsChanged?.Invoke();
    }
    #endregion
    #region
    /// <summary>
    /// 指定した対象（送ダメージ or 受ダメージ）に有効なバフの倍率をすべて掛け合わせて返す。
    /// バフがない場合は 1.0 を返す。
    /// </summary>
    /// <param name="target">集計するバフの対象種別</param>
    /// <returns>合成済みのダメージ倍率</returns>
    private float GetDamageMultiplier(DamageBuffTarget target)
    {
        float multiplier = 1f;
        for (int i = 0; i < _damageBuffs.Count; i++)
        {
            DamageBuff buff = _damageBuffs[i];
            if (buff.Target != target) continue;

            multiplier *= buff.Multiplier;
        }

        return multiplier;
    }
    #endregion
    #region
    /// <summary>
    /// 指定 ID のバフを線形探索で返す。見つからない場合は null。
    /// </summary>
    /// <param name="id">検索するバフの ID</param>
    /// <returns>見つかったバフのインスタンス。存在しない場合は null。</returns>
    private DamageBuff FindDamageBuff(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        for (int i = 0; i < _damageBuffs.Count; i++)
        {
            if (_damageBuffs[i].Id == id)
                return _damageBuffs[i];
        }

        return null;
    }
    #endregion
    #region
    /// <summary>
    /// 一時的な被弾状態のタイマーを更新する。
    /// タイマーが 0 以下になると <see cref="_stateBeforeTimedState"/> に復帰する。
    /// </summary>
    /// <param name="deltaTime">経過時間（秒）</param>
    private void TickTimedHitReactionState(float deltaTime)
    {
        if (!_hasTimedState) return;

        _timedStateTimer -= deltaTime;
        if (_timedStateTimer > 0f) return;

        // タイマー終了 → 元の状態に戻す
        _hitReactionState = _stateBeforeTimedState;
        _timedStateTimer  = 0f;
        _hasTimedState    = false;
    }
    #endregion
    #region
    /// <summary>
    /// 子オブジェクトを含む Renderer を取得し、マテリアルのベースカラーをキャッシュする。
    /// Awake 時に一度だけ呼び出す。
    /// </summary>
    private void CacheRenderers()
    {
        _renderers  = GetComponentsInChildren<Renderer>();
        _baseColors = new Color[_renderers.Length];
        _rendererBlocks.Clear();

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer       = _renderers[i];
            Material sharedMaterial = renderer.sharedMaterial;

            // URP (_BaseColor) → Built-in (_Color) の順でベースカラーを取得する
            _baseColors[i] = sharedMaterial != null && sharedMaterial.HasProperty(BaseColorId)
                ? sharedMaterial.GetColor(BaseColorId)
                : sharedMaterial != null && sharedMaterial.HasProperty(ColorId)
                    ? sharedMaterial.GetColor(ColorId)
                    : Color.white;

            _rendererBlocks.Add(new MaterialPropertyBlock());
        }

        _lastAppliedHitReactionState = _hitReactionState;
    }
    #endregion
    #region
    /// <summary>
    /// 被弾状態に応じたマテリアルカラーを更新する。
    /// 無敵状態の場合はシアン色で点滅し、それ以外は元のベースカラーに戻す。
    /// 状態が変化していない（かつ点滅中でない）場合は処理をスキップする。
    /// </summary>
    private void UpdateHitReactionVisual()
    {
        if (_renderers == null || _renderers.Length == 0) return;

        bool shouldBlink = IsInvincible;

        // 点滅中でなく、被弾状態も変化していなければ更新不要
        if (!shouldBlink && _lastAppliedHitReactionState == _hitReactionState) return;

        for (int i = 0; i < _renderers.Length; i++)
        {
            Renderer renderer = _renderers[i];
            if (renderer == null) continue;

            MaterialPropertyBlock block = _rendererBlocks[i];
            renderer.GetPropertyBlock(block);

            // 無敵中はベースカラーとシアン色の間でサイン波的に補間する
            Color color = shouldBlink
                ? Color.Lerp(_baseColors[i], _invincibleBlinkColor,
                    Mathf.PingPong(Time.time * _invincibleBlinkSpeed, 1f))
                : _baseColors[i];

            // URP と Built-in の両シェーダーに対応するため両プロパティに設定する
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            renderer.SetPropertyBlock(block);
        }

        _lastAppliedHitReactionState = _hitReactionState;
    }
    #endregion
}

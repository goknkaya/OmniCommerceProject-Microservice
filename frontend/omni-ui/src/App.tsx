import { useEffect, useMemo, useRef, useState } from "react";

type Summary = {
  total: number;
  last24h: number;
  byCurrency: { currency: string; count: number }[];
  last10: {
    id: string;
    orderId: string;
    customerId: string;
    amount: number;
    currency: string;
    createdAt: string;
    receivedAt: string;
  }[];
};

const SUMMARY_URL = "https://localhost:7214/reports/summary";

function fmtTR(dateIso: string) {
  return new Date(dateIso).toLocaleString("tr-TR");
}

export default function App() {
  const [summary, setSummary] = useState<Summary | null>(null);
  const [error, setError] = useState<string>("");
  const [loading, setLoading] = useState<boolean>(false);

  const [autoRefresh, setAutoRefresh] = useState<boolean>(true);
  const [refreshSeconds, setRefreshSeconds] = useState<number>(30);

  const [lastUpdatedAt, setLastUpdatedAt] = useState<Date | null>(null);

  const timerRef = useRef<number | null>(null);

  const healthText = useMemo(() => {
    if (loading) return "Loading...";
    if (error) return "Error";
    if (!summary) return "Idle";
    return "OK";
  }, [loading, error, summary]);

  async function loadSummary() {
    try {
      setError("");
      setLoading(true);

      // cache busting (bazı durumlarda faydalı)
      const url = `${SUMMARY_URL}?t=${Date.now()}`;

      const r = await fetch(url);
      if (!r.ok) throw new Error(`HTTP ${r.status}`);

      const json = (await r.json()) as Summary;
      setSummary(json);
      setLastUpdatedAt(new Date());
    } catch (e: any) {
      setError(String(e?.message ?? e));
    } finally {
      setLoading(false);
    }
  }

  // initial load
  useEffect(() => {
    loadSummary();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // auto refresh effect
  useEffect(() => {
    // temizlik
    if (timerRef.current) {
      window.clearInterval(timerRef.current);
      timerRef.current = null;
    }

    if (!autoRefresh) return;

    const safeSeconds = Math.min(Math.max(refreshSeconds, 5), 300); // 5-300 arası
    timerRef.current = window.setInterval(() => {
      loadSummary();
    }, safeSeconds * 1000);

    return () => {
      if (timerRef.current) {
        window.clearInterval(timerRef.current);
        timerRef.current = null;
      }
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [autoRefresh, refreshSeconds]);

  return (
    <div style={{ fontFamily: "Arial", padding: 24, background: "#1f1f1f", minHeight: "100vh", color: "#f2f2f2" }}>
      <div style={{ display: "flex", alignItems: "flex-start", justifyContent: "space-between", gap: 16 }}>
        <div>
          <h1 style={{ margin: 0, fontSize: 54, fontWeight: 800 }}>OmniCommerce Dashboard</h1>
          <div style={{ marginTop: 8, opacity: 0.8 }}>
            Status: <b>{healthText}</b>
            {lastUpdatedAt && (
              <>
                {" "}
                • Last updated: <b>{lastUpdatedAt.toLocaleString("tr-TR")}</b>
              </>
            )}
          </div>
        </div>

        {/* Controls */}
        <div style={{ display: "flex", flexDirection: "column", gap: 10, minWidth: 260 }}>
          <button
            onClick={loadSummary}
            disabled={loading}
            style={{
              cursor: loading ? "not-allowed" : "pointer",
              padding: "10px 14px",
              borderRadius: 10,
              border: "1px solid #444",
              background: loading ? "#2a2a2a" : "#2f2f2f",
              color: "#fff",
              fontWeight: 700,
            }}
            title="Raporu yeniden çek"
          >
            {loading ? "Refreshing..." : "Refresh"}
          </button>

          <div
            style={{
              border: "1px solid #3b3b3b",
              borderRadius: 10,
              padding: 12,
              background: "#242424",
            }}
          >
            <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", gap: 10 }}>
              <div style={{ fontWeight: 700 }}>Auto refresh</div>
              <label style={{ display: "flex", alignItems: "center", gap: 8, cursor: "pointer" }}>
                <input
                  type="checkbox"
                  checked={autoRefresh}
                  onChange={(e) => setAutoRefresh(e.target.checked)}
                  style={{ transform: "scale(1.1)" }}
                />
                <span style={{ opacity: 0.9 }}>{autoRefresh ? "ON" : "OFF"}</span>
              </label>
            </div>

            <div style={{ marginTop: 10, display: "flex", alignItems: "center", gap: 10 }}>
              <span style={{ opacity: 0.85 }}>Every</span>
              <input
                type="number"
                min={5}
                max={300}
                value={refreshSeconds}
                onChange={(e) => setRefreshSeconds(Number(e.target.value))}
                disabled={!autoRefresh}
                style={{
                  width: 90,
                  padding: "8px 10px",
                  borderRadius: 10,
                  border: "1px solid #444",
                  background: !autoRefresh ? "#2a2a2a" : "#1f1f1f",
                  color: "#fff",
                }}
              />
              <span style={{ opacity: 0.85 }}>sec</span>
            </div>

            <div style={{ marginTop: 8, fontSize: 12, opacity: 0.75 }}>
              (Min 5, Max 300)
            </div>
          </div>
        </div>
      </div>

      {/* Error */}
      {error && (
        <div
          style={{
            marginTop: 16,
            background: "#3a0f14",
            border: "1px solid #ff4d4f55",
            padding: 12,
            borderRadius: 10,
          }}
        >
          <b>Hata:</b> {error}
          <div style={{ marginTop: 6, opacity: 0.9 }}>
            Not: Büyük ihtimalle CORS/HTTPS kaynaklı olur. CatalogService çalışıyor mu? (7214)
          </div>
        </div>
      )}

      {/* Loading placeholder */}
      {!summary && !error && (
        <div style={{ marginTop: 18, opacity: 0.85 }}>
          Yükleniyor...
        </div>
      )}

      {/* Cards + table */}
      {summary && (
        <>
          <div style={{ display: "flex", gap: 12, marginTop: 18, flexWrap: "wrap" }}>
            <div style={{ border: "1px solid #555", padding: 14, borderRadius: 12, minWidth: 220, background: "#232323" }}>
              <div style={{ opacity: 0.9, fontWeight: 700 }}>Total Received Orders</div>
              <div style={{ fontSize: 34, fontWeight: 800, marginTop: 4 }}>{summary.total}</div>
            </div>

            <div style={{ border: "1px solid #555", padding: 14, borderRadius: 12, minWidth: 140, background: "#232323" }}>
              <div style={{ opacity: 0.9, fontWeight: 700 }}>Last 24h</div>
              <div style={{ fontSize: 34, fontWeight: 800, marginTop: 4 }}>{summary.last24h}</div>
            </div>
          </div>

          <h2 style={{ marginTop: 26, fontSize: 34, marginBottom: 8 }}>By Currency</h2>
          <ul style={{ marginTop: 8 }}>
            {summary.byCurrency.map((x) => (
              <li key={x.currency} style={{ marginBottom: 6 }}>
                <b>{x.currency}</b> - {x.count}
              </li>
            ))}
          </ul>

          <h2 style={{ marginTop: 26, fontSize: 34 }}>Last 10</h2>

          <table cellPadding={10} style={{ borderCollapse: "collapse", width: "100%", marginTop: 10 }}>
            <thead>
              <tr>
                <th align="left" style={{ borderBottom: "1px solid #666" }}>OrderId</th>
                <th align="left" style={{ borderBottom: "1px solid #666" }}>Customer</th>
                <th align="left" style={{ borderBottom: "1px solid #666" }}>Status</th>
                <th align="left" style={{ borderBottom: "1px solid #666" }}>Amount</th>
                <th align="left" style={{ borderBottom: "1px solid #666" }}>ReceivedAt</th>
                <th align="left" style={{ borderBottom: "1px solid #666" }}>CreatedAt</th>
              </tr>
            </thead>

            <tbody>
              {summary.last10.map((x) => {
                const isFail = x.customerId?.toLowerCase().includes("fail");

                // rozet
                const badgeStyle: React.CSSProperties = {
                  display: "inline-flex",
                  justifyContent: "center",
                  alignItems: "center",
                  minWidth: 92,          
                  textAlign: "center",
                  padding: "4px 10px",
                  borderRadius: 999,
                  fontSize: 12,
                  fontWeight: 800,
                  letterSpacing: 0.5,
                  background: isFail ? "#ff4d4f22" : "#52c41a22",
                  color: isFail ? "#ff7875" : "#73d13d",
                  border: `1px solid ${isFail ? "#ff4d4f55" : "#52c41a55"}`,
                };

                return (
                  <tr
                    key={x.id}
                    style={{
                      background: isFail ? "#3a0f14" : "#0f2a16",
                    }}
                  >
                    <td style={{ borderBottom: "1px solid #333" }}>{x.orderId}</td>
                    <td style={{ borderBottom: "1px solid #333" }}>{x.customerId}</td>

                    {/* ✅ Status ayrı kolon + rozetler alt alta aynı hizaya oturur */}
                    <td style={{ borderBottom: "1px solid #333" }}>
                      <span style={badgeStyle}>{isFail ? "FAILED" : "SUCCESS"}</span>
                    </td>

                    <td style={{ borderBottom: "1px solid #333" }}>
                      {x.amount} {x.currency}
                    </td>

                    <td style={{ borderBottom: "1px solid #333" }}>{fmtTR(x.receivedAt)}</td>
                    <td style={{ borderBottom: "1px solid #333" }}>{fmtTR(x.createdAt)}</td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </>
      )}
    </div>
  );
}

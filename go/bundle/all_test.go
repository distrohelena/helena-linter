package bundle

import (
	"slices"
	"strings"
	"testing"
)

func TestRecommendedExportsSupportedRuleSet(t *testing.T) {
	got := Recommended()

	wantNames := []string{
		"controlblockfollowingspacing",
		"declarationleadingspacing",
		"declarationspacing",
		"earlyreturn",
		"exitspacing",
		"ifelseifchain",
		"iffollowingspacing",
		"ifleadingspacing",
		"multilineblocklayout",
		"redundantelseif",
	}

	if len(got) != len(wantNames) {
		t.Fatalf("Recommended() returned %d analyzers, want %d", len(got), len(wantNames))
	}

	names := make([]string, 0, len(got))
	seen := make(map[string]struct{}, len(got))
	for i, analyzer := range got {
		if analyzer == nil {
			t.Fatalf("Recommended()[%d] is nil", i)
		}
		names = append(names, analyzer.Name)
		if analyzer.Name != wantNames[i] {
			t.Fatalf("Recommended()[%d].Name = %q, want %q", i, analyzer.Name, wantNames[i])
		}
		if _, ok := seen[analyzer.Name]; ok {
			t.Fatalf("Recommended() contains duplicate analyzer name %q", analyzer.Name)
		}
		seen[analyzer.Name] = struct{}{}
	}

	if !slices.Equal(names, wantNames) {
		t.Fatalf("Recommended() analyzer names = %v, want %v", names, wantNames)
	}
}

func TestRecommendedDoesNotExportControlBodyBraces(t *testing.T) {
	for _, analyzer := range Recommended() {
		if analyzer == nil {
			continue
		}
		if analyzer.Name == "controlbodybraces" {
			t.Fatal("Recommended() unexpectedly exports control-body-braces")
		}
		if strings.Contains(analyzer.Doc, "control-body-braces") {
			t.Fatalf("Recommended() analyzer %q doc references control-body-braces", analyzer.Name)
		}
	}
}


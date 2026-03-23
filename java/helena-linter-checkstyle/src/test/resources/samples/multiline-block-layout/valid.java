class Test {
    void methodCase() {
        System.out.println();
    }

    void emptyMethodCase() {}

    void commentOnlyCase() {
        // comment-only blocks are allowed
    }

    void emptyIfCase(boolean flag) {
        if (flag) {}
    }

    void multilineIfCase(boolean flag) {
        if (flag) {
            System.out.println(flag);
        }
    }

    void tryCase() {
        try {
            System.out.println();
        } catch (Exception ex) {}
    }

    void initializerCase() {
        int[] values = new int[] { 1, 2 };
        int[] more = { 3, 4 };
    }
}

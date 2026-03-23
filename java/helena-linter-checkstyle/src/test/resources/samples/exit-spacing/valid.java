class Test {
    void returnCase() {
        int count = 1;

        return;
    }

    void throwCase() {
        int count = 1;

        throw new IllegalStateException();
    }

    void breakCase() {
        while (true) {
            int count = 1;

            break;
        }
    }

    void continueCase() {
        for (int i = 0; i < 1; i++) {
            int count = i;

            continue;
        }
    }
}

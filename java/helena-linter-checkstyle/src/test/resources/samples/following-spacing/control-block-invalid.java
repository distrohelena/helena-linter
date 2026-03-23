class FollowingSpacingControlBlockInvalid {
    void test(boolean flag, int value) {
        for (int i = 0; i < value; i++) {
            work();
        }
        while (flag) {
            work();
        }
        do {
            work();
        } while (flag);
        switch (value) {
            case 1:
                work();
                break;
            default:
                break;
        }
        try {
            work();
        } catch (RuntimeException ex) {
            work();
        } finally {
            work();
        }
        work();
    }

    void work() {
    }
}

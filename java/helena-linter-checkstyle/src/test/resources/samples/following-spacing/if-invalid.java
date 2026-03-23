class FollowingSpacingIfInvalid {
    void test(boolean flag) {
        if (flag) {
            work();
        } else if (!flag) {
            work();
        } else {
            work();
        }
        work();
    }

    void work() {
    }
}

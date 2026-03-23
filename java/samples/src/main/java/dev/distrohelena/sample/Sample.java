package dev.distrohelena.sample;

/**
 * Provides a sample consumer that intentionally violates Helena rules so the local Checkstyle
 * package can be verified end to end.
 */
public class Sample {

    /**
     * Creates spacing and brace violations around local statements and a simple {@code if} body.
     *
     * @param value the input value used to create predictable statement ordering.
     * @return the final value after the sample statements run.
     */
    public int spacingAndBraces(int value) {
        int first = value;
        System.out.println(first);
        int second = first + 1;
        System.out.println(second);
        if (second > 0)
            second++;
        System.out.println("done");

        return second;
    }

    /**
     * Creates control-flow spacing violations and an {@code if} chain that should have become
     * {@code else if}.
     *
     * @param value the input value used to drive the branching examples.
     */
    public void controlFlowSpacing(int value) {
        if (value < 0) {
            return;
        }
        if (value == 0) {
            value++;
        }

        while (value > 0)
            value--;
        int afterLoop = value;
        System.out.println(afterLoop);
    }

    /**
     * Creates a redundant {@code else if} branch and a wrapped happy-path block that should be
     * flattened into an early return.
     *
     * @param value the input value used to exercise the final rule set.
     * @return the adjusted value after the sample statements run.
     */
    public int branchingExamples(int value) {
        if (value < 10) {
            value++;
        } else if (value >= 10) {
            value--;
        }

        if (value > 100) {
            value = 100;
        }
        return value;
    }

    /**
     * Creates a complementary boolean {@code else if} branch so the redundant-branch rule is
     * exercised by the sample consumer.
     *
     * @param flag the boolean value used to create a complementary branch pair.
     */
    public void redundantElseIfExample(boolean flag) {
        if (!flag) {
            System.out.println("flag is false");
        } else if (flag) {
            System.out.println("flag is true");
        }
    }
}
